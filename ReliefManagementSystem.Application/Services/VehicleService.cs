using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Vehicle.DTOs.Request;
using ReliefManagementSystem.Application.Features.Vehicle.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VehicleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Create a new Vehicle
        public async Task<VehicleResponse> CreateVehicleAsync(
            CreateVehicleRequest request,
            Guid userId,
            CancellationToken cancellationToken
            )
        {             
            // Validate Vehicle Type exists
            var vehicleType = await _unitOfWork.VehicleTypes.GetByIdAsync(request.VehicleTypeId);
            if (vehicleType == null || vehicleType.IsDeleted)
            {
                throw new Exception("Không thấy loại phương tiện");
            }

            // Check duplicate license plate
            //if (await _unitOfWork.Vehicles.IsLicensePlateExistsAsync(request.LicensePlate))
            //{
            //    throw new Exception("License Plate already exists");
            //}

            var vehicle = new Vehicle
            {
                VehicleTypeId = request.VehicleTypeId,
                LicensePlate = request.LicensePlate.ToUpper(),
                CreatedBy = userId,
                TeamUsed = request.TeamUsed,
                Status = VehicleStatus.Free,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Vehicles.AddAsync(vehicle);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapToVehicleResponse(vehicle.VehicleId, cancellationToken);
        }

        // Get Vehicle by Id
        public async Task<VehicleResponse> GetVehicleByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdWithDetailsAsync(id);
            if (vehicle == null)
            {
                throw new Exception("Không thấy phương tiện");
            }

            return MapToResponse(vehicle);
        }

        // Get all active Vehicles
        public async Task<Pagination<VehicleResponse>> GetAllVehiclesAsync(
            SearchVehicleRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.Vehicles.GetQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                query = query.Where(v =>
                    (v.LicensePlate ?? string.Empty).Contains(keyword) ||
                    (v.TeamUsed ?? string.Empty).Contains(keyword) ||
                    (v.VehicleType != null && (v.VehicleType.TypeName ?? string.Empty).Contains(keyword)));
            }

            query = query.OrderByDescending(v => v.CreatedAt);

            var pagedVehicles = await Pagination<Vehicle>.ToPagedList(query, request.PageIndex, request.PageSize);
            var items = pagedVehicles.Items!.Select(MapToResponse).ToList();

            return new Pagination<VehicleResponse>(items, pagedVehicles.TotalCount, pagedVehicles.CurrentPage, pagedVehicles.PageSize);
        }

        // Get Vehicles by Status
        public async Task<IReadOnlyList<VehicleResponse>> GetVehiclesByStatusAsync(
            int status,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(typeof(VehicleStatus), status))
            {
                throw new Exception("Giá trị status không hợp lệ");
            }

            var vehicles = await _unitOfWork.Vehicles.GetByStatusAsync((VehicleStatus)status);
            return vehicles.Select(MapToResponse).ToList();
        }

        // Get Vehicles created by user
        public async Task<IReadOnlyList<VehicleResponse>> GetMyVehiclesAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var vehicles = await _unitOfWork.Vehicles.GetByCreatorAsync(userId);
            return vehicles.Select(MapToResponse).ToList();
        }

        // Update Vehicle
        public async Task<VehicleResponse> UpdateVehicleAsync(
            Guid id,
            UpdateVehicleRequest request,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdWithDetailsAsync(id);
            if (vehicle == null)
            {
                throw new Exception("Không thấy phương tiện");
            }

            // Check authorization - only creator can update
            if (vehicle.CreatedBy != userId)
            {
                throw new Exception("Bạn không có quyền cập nhật phương tiện");
            }

            // Validate Vehicle Type exists
            var vehicleType = await _unitOfWork.VehicleTypes.GetByIdAsync(request.VehicleTypeId);
            if (vehicleType == null || vehicleType.IsDeleted)
            {
                throw new Exception("Không thấy loại phương tiện");
            }

            // Check duplicate license plate (exclude current vehicle)
            //if (await _unitOfWork.Vehicles.IsLicensePlateExistsAsync(request.LicensePlate, id))
            //{
            //    throw new Exception("License Plate already exists");
            //}

            if (!Enum.IsDefined(typeof(VehicleStatus), request.Status))
            {
                throw new Exception("Giá trị status không hợp lệ");
            }

            vehicle.VehicleTypeId = request.VehicleTypeId;
            vehicle.LicensePlate = request.LicensePlate.ToUpper();
            vehicle.TeamUsed = request.TeamUsed;
            vehicle.Status = (VehicleStatus)request.Status;
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Vehicles.UpdateAsync(vehicle);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapToVehicleResponse(id, cancellationToken);
        }

        // Delete Vehicle 
        public async Task<bool> DeleteVehicleAsync(
            Guid id,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
            if (vehicle == null || vehicle.IsDeleted)
            {
                throw new Exception("Không thấy phương tiện");
            }

            // Check authorization - only creator can delete
            if (vehicle.CreatedBy != userId)
            {
                throw new Exception("Bạn không có quyền đề xoá phương tiện");
            }

            vehicle.IsDeleted = true;
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Vehicles.UpdateAsync(vehicle);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        // Helper method to map Vehicle entity to VehicleResponse DTO
        private async Task<VehicleResponse> MapToVehicleResponse(Guid vehicleId, CancellationToken cancellationToken)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdWithDetailsAsync(vehicleId);
            return MapToResponse(vehicle!);
        }

        private VehicleResponse MapToResponse(Vehicle vehicle)
        {
            return new VehicleResponse
            {
                VehicleId = vehicle.VehicleId,
                VehicleTypeId = vehicle.VehicleTypeId,
                VehicleTypeName = vehicle.VehicleType?.TypeName ?? string.Empty,
                LicensePlate = vehicle.LicensePlate,
                CreatedBy = vehicle.CreatedBy,
                CreatorName = vehicle.Creator?.UserName ?? string.Empty,
                TeamUsed = vehicle.TeamUsed,
                Status = (int)vehicle.Status,
                StatusName = vehicle.Status.ToString(),
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt
            };
        }

    }
}
