using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.VehicleType.DTOs.Request;
using ReliefManagementSystem.Application.Features.VehicleType.DTOs.Response;
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
    public class VehicleTypeService : IVehicleTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        public VehicleTypeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<VehicleTypeResponse> CreateVehicleTypeAsync(
            CreateVehicleTypeRequest request,
            CancellationToken cancellationToken = default)
        {
            // Check duplicate type name
            if (await _unitOfWork.VehicleTypes.IsTypeNameExistsAsync(request.TypeName))
            {
                throw new Exception("Vehicle Type name already exists");
            }

            var vehicleType = new VehicleType
            {
                TypeName = request.TypeName,
                DefaultCapacity = request.DefaultCapacity,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.VehicleTypes.AddAsync(vehicleType);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(vehicleType);
        }
        public async Task<VehicleTypeDetailResponse> GetVehicleTypeByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var vehicleType = await _unitOfWork.VehicleTypes.GetByIdWithVehiclesAsync(id);
            if (vehicleType == null)
            {
                throw new Exception("Vehicle Type not found");
            }

            var totalVehicles = vehicleType.Vehicles.Count;
            var freeVehicles = vehicleType.Vehicles.Count(v => v.Status == VehicleStatus.Free);
            var busyVehicles = vehicleType.Vehicles.Count(v => v.Status == VehicleStatus.Busy);

            return new VehicleTypeDetailResponse
            {
                VehicleTypeId = vehicleType.VehicleTypeId,
                TypeName = vehicleType.TypeName,
                DefaultCapacity = vehicleType.DefaultCapacity,
                Description = vehicleType.Description,
                TotalVehicles = totalVehicles,
                FreeVehicles = freeVehicles,
                BusyVehicles = busyVehicles,
                CreatedAt = vehicleType.CreatedAt,
                UpdatedAt = vehicleType.UpdatedAt
            };
        }

        public async Task<IReadOnlyList<VehicleTypeResponse>> GetAllVehicleTypesAsync(
            CancellationToken cancellationToken = default)
        {
            var vehicleTypes = await _unitOfWork.VehicleTypes.GetAllActiveAsync();
            return vehicleTypes.Select(MapToResponse).ToList();
        }

        public async Task<VehicleTypeResponse> UpdateVehicleTypeAsync(
            Guid id,
            UpdateVehicleTypeRequest request,
            CancellationToken cancellationToken = default)
        {
            var vehicleType = await _unitOfWork.VehicleTypes.GetByIdAsync(id);
            if (vehicleType == null || vehicleType.IsDeleted)
            {
                throw new Exception("Vehicle Type not found");
            }

            // Check duplicate type name (exclude current type)
            if (await _unitOfWork.VehicleTypes.IsTypeNameExistsAsync(request.TypeName, id))
            {
                throw new Exception("Vehicle Type name already exists");
            }

            vehicleType.TypeName = request.TypeName;
            vehicleType.DefaultCapacity = request.DefaultCapacity;
            vehicleType.Description = request.Description;
            vehicleType.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.VehicleTypes.UpdateAsync(vehicleType);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(vehicleType);
        }

        public async Task<bool> DeleteVehicleTypeAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var vehicleType = await _unitOfWork.VehicleTypes.GetByIdWithVehiclesAsync(id);
            if (vehicleType == null || vehicleType.IsDeleted)
            {
                throw new Exception("Vehicle Type not found");
            }

            // Check if has active vehicles
            if (vehicleType.Vehicles.Any())
            {
                throw new Exception("Cannot delete Vehicle Type that has vehicles. Please delete or reassign vehicles first.");
            }

            vehicleType.IsDeleted = true;
            vehicleType.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.VehicleTypes.UpdateAsync(vehicleType);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        private VehicleTypeResponse MapToResponse(VehicleType vehicleType)
        {
            return new VehicleTypeResponse
            {
                VehicleTypeId = vehicleType.VehicleTypeId,
                TypeName = vehicleType.TypeName,
                DefaultCapacity = vehicleType.DefaultCapacity,
                Description = vehicleType.Description,
                CreatedAt = vehicleType.CreatedAt,
                UpdatedAt = vehicleType.UpdatedAt
            };
        }


    }
}
