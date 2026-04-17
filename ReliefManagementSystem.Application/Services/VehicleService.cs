using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Vehicle.DTOs.Request;
using ReliefManagementSystem.Application.Features.Vehicle.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using Microsoft.EntityFrameworkCore;
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
            bool isManager,
            bool isModerator,
            CancellationToken cancellationToken
            )
        {
            Guid? effectiveStationId = request.ReliefStationId;
            if (isModerator)
            {
                effectiveStationId = await GetModeratorStationIdAsync(userId, cancellationToken);
            }

            // Validate Vehicle Type exists
            var vehicleType = await _unitOfWork.VehicleTypes.GetByIdAsync(request.VehicleTypeId);
            if (vehicleType == null || vehicleType.IsDeleted)
            {
                throw new Exception("Không thấy loại phương tiện");
            }

            // Check duplicate license plate
            if (await _unitOfWork.Vehicles.IsLicensePlateExistsAsync(request.LicensePlate.ToUpper()))
            {
                throw new Exception("License Plate already exists");
            }

            if (effectiveStationId.HasValue)
            {
                var station = await _unitOfWork.ReliefStations.GetByIdAsync(effectiveStationId.Value);
                if (station == null)
                {
                    throw new Exception("Không thấy trạm cứu trợ");
                }
            }

            if (request.TeamId.HasValue)
            {
                if (!effectiveStationId.HasValue)
                {
                    throw new Exception("Cần gán trạm trước khi gán team cho phương tiện");
                }

                var team = await _unitOfWork.Teams.GetByIdAsync(request.TeamId.Value);
                if (team == null)
                {
                    throw new Exception("Không thấy team");
                }

                var assignment = await _unitOfWork.ReliefStationTeams
                    .GetByStationAndTeamAsync(effectiveStationId.Value, request.TeamId.Value, cancellationToken);

                if (assignment == null || assignment.Status != ReliefTeamAssignmentStatus.Approved)
                {
                    throw new Exception("Team chưa được duyệt tại trạm này");
                }
            }

            var vehicle = new Vehicle
            {
                VehicleTypeId = request.VehicleTypeId,
                LicensePlate = request.LicensePlate.ToUpper(),
                CreatedBy = userId,
                ReliefStationId = effectiveStationId,
                TeamId = request.TeamId,
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
            Guid userId,
            bool isManager,
            bool isModerator,
            CancellationToken cancellationToken = default)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdWithDetailsAsync(id);
            if (vehicle == null)
            {
                throw new Exception("Không thấy phương tiện");
            }

            if (isModerator)
            {
                var moderatorStationId = await GetModeratorStationIdAsync(userId, cancellationToken);
                if (vehicle.ReliefStationId != moderatorStationId)
                {
                    throw new Exception("Bạn chỉ được xem phương tiện trong trạm của mình");
                }
            }

            return MapToResponse(vehicle);
        }

        // Get all active Vehicles
        public async Task<Pagination<VehicleResponse>> GetAllVehiclesAsync(
            SearchVehicleRequest request,
            Guid userId,
            bool isManager,
            bool isModerator,
            CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.Vehicles.GetQueryable();

            if (isModerator)
            {
                var moderatorStationId = await GetModeratorStationIdAsync(userId, cancellationToken);
                query = query.Where(v => v.ReliefStationId == moderatorStationId);
            }
            else if (request.ReliefStationId.HasValue)
            {
                query = query.Where(v => v.ReliefStationId == request.ReliefStationId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                query = query.Where(v =>
                    (v.LicensePlate ?? string.Empty).Contains(keyword) ||
                    (v.Team != null && (v.Team.Name ?? string.Empty).Contains(keyword)) ||
                    (v.ReliefStation != null && (v.ReliefStation.Name ?? string.Empty).Contains(keyword)) ||
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
            Guid userId,
            bool isManager,
            bool isModerator,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(typeof(VehicleStatus), status))
            {
                throw new Exception("Giá trị status không hợp lệ");
            }

            var vehicles = await _unitOfWork.Vehicles.GetByStatusAsync((VehicleStatus)status);

            if (isModerator)
            {
                var moderatorStationId = await GetModeratorStationIdAsync(userId, cancellationToken);
                vehicles = vehicles.Where(v => v.ReliefStationId == moderatorStationId).ToList();
            }

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
            bool isManager,
            bool isModerator,
            CancellationToken cancellationToken = default)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdWithDetailsAsync(id);
            if (vehicle == null)
            {
                throw new Exception("Không thấy phương tiện");
            }

            await EnsureCanManageVehicleAsync(vehicle, userId, isManager, isModerator, cancellationToken);

            // Validate Vehicle Type exists
            var vehicleType = await _unitOfWork.VehicleTypes.GetByIdAsync(request.VehicleTypeId);
            if (vehicleType == null || vehicleType.IsDeleted)
            {
                throw new Exception("Không thấy loại phương tiện");
            }

            // Check duplicate license plate (exclude current vehicle)
            if (await _unitOfWork.Vehicles.IsLicensePlateExistsAsync(request.LicensePlate.ToUpper(), id))
            {
                throw new Exception("License Plate already exists");
            }

            if (!Enum.IsDefined(typeof(VehicleStatus), request.Status))
            {
                throw new Exception("Giá trị status không hợp lệ");
            }

            vehicle.VehicleTypeId = request.VehicleTypeId;
            vehicle.LicensePlate = request.LicensePlate.ToUpper();
            if (request.TeamId.HasValue)
            {
                if (!vehicle.ReliefStationId.HasValue)
                {
                    throw new Exception("Phương tiện chưa được gán trạm, không thể gán team");
                }

                var team = await _unitOfWork.Teams.GetByIdAsync(request.TeamId.Value);
                if (team == null)
                {
                    throw new Exception("Không thấy team");
                }

                var assignment = await _unitOfWork.ReliefStationTeams
                    .GetByStationAndTeamAsync(vehicle.ReliefStationId.Value, request.TeamId.Value, cancellationToken);

                if (assignment == null || assignment.Status != ReliefTeamAssignmentStatus.Approved)
                {
                    throw new Exception("Team chưa được duyệt tại trạm của phương tiện");
                }
            }

            vehicle.TeamId = request.TeamId;
            vehicle.Status = (VehicleStatus)request.Status;
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Vehicles.UpdateAsync(vehicle);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapToVehicleResponse(id, cancellationToken);
        }

        public async Task<VehicleResponse> AssignVehicleToStationAsync(
            Guid vehicleId,
            Guid stationId,
            Guid userId,
            bool isManager,
            CancellationToken cancellationToken = default)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdWithDetailsAsync(vehicleId);
            if (vehicle == null)
            {
                throw new Exception("Không thấy phương tiện");
            }

            if (!isManager)
            {
                throw new Exception("Bạn không có quyền gán trạm cho phương tiện này");
            }

            var station = await _unitOfWork.ReliefStations.GetByIdAsync(stationId);
            if (station == null)
            {
                throw new Exception("Không thấy trạm cứu trợ");
            }

            if (vehicle.TeamId.HasValue)
            {
                var assignment = await _unitOfWork.ReliefStationTeams
                    .GetByStationAndTeamAsync(stationId, vehicle.TeamId.Value, cancellationToken);

                if (assignment == null || assignment.Status != ReliefTeamAssignmentStatus.Approved)
                {
                    throw new Exception("Team hiện tại của phương tiện chưa được duyệt ở trạm đích");
                }
            }

            vehicle.ReliefStationId = stationId;
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Vehicles.UpdateAsync(vehicle);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapToVehicleResponse(vehicle.VehicleId, cancellationToken);
        }

        public async Task<VehicleResponse> AssignVehicleToTeamAsync(
            Guid vehicleId,
            Guid teamId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var moderatorProfile = await _unitOfWork.ModeratorProfiles.GetByUserIdAsync(userId, cancellationToken);
            if (moderatorProfile == null || !moderatorProfile.ReliefStationId.HasValue)
            {
                throw new Exception("Bạn không thuộc trạm nào để gán team");
            }

            var vehicle = await _unitOfWork.Vehicles.GetByIdWithDetailsAsync(vehicleId);
            if (vehicle == null)
            {
                throw new Exception("Không thấy phương tiện");
            }

            if (!vehicle.ReliefStationId.HasValue)
            {
                throw new Exception("Phương tiện chưa được gán trạm");
            }

            if (vehicle.ReliefStationId.Value != moderatorProfile.ReliefStationId.Value)
            {
                throw new Exception("Bạn chỉ được gán team cho phương tiện trong trạm của mình");
            }

            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            if (team == null)
            {
                throw new Exception("Không thấy team");
            }

            var assignment = await _unitOfWork.ReliefStationTeams
                .GetByStationAndTeamAsync(vehicle.ReliefStationId.Value, teamId, cancellationToken);

            if (assignment == null || assignment.Status != ReliefTeamAssignmentStatus.Approved)
            {
                throw new Exception("Team chưa được duyệt tại trạm của phương tiện");
            }

            vehicle.TeamId = teamId;
            vehicle.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Vehicles.UpdateAsync(vehicle);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapToVehicleResponse(vehicle.VehicleId, cancellationToken);
        }

        public async Task<object> GetVehicleCountsAsync(Guid? stationId, CancellationToken cancellationToken = default)
        {
            if (stationId.HasValue)
            {
                var station = await _unitOfWork.ReliefStations.GetByIdAsync(stationId.Value);
                if (station == null)
                {
                    throw new Exception("Không thấy trạm cứu trợ");
                }
            }

            var total = await _unitOfWork.Vehicles.GetCountAsync(stationId, null);
            var free = await _unitOfWork.Vehicles.GetCountAsync(stationId, VehicleStatus.Free);
            var busy = await _unitOfWork.Vehicles.GetCountAsync(stationId, VehicleStatus.Busy);

            var unassignedStation = await _unitOfWork.Vehicles.GetQueryable()
                .Where(v => v.ReliefStationId == null)
                .CountAsync(cancellationToken);

            return new
            {
                total,
                free,
                busy,
                unassignedStation
            };
        }

        // Delete Vehicle 
        public async Task<bool> DeleteVehicleAsync(
            Guid id,
            Guid userId,
            bool isManager,
            bool isModerator,
            CancellationToken cancellationToken = default)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
            if (vehicle == null || vehicle.IsDeleted)
            {
                throw new Exception("Không thấy phương tiện");
            }

            await EnsureCanManageVehicleAsync(vehicle, userId, isManager, isModerator, cancellationToken);

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
                ReliefStationId = vehicle.ReliefStationId,
                ReliefStationName = vehicle.ReliefStation?.Name,
                TeamId = vehicle.TeamId,
                TeamName = vehicle.Team?.Name,
                Status = (int)vehicle.Status,
                StatusName = vehicle.Status.ToString(),
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt
            };
        }

        private async Task<Guid> GetModeratorStationIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var moderatorProfile = await _unitOfWork.ModeratorProfiles.GetByUserIdAsync(userId, cancellationToken);
            if (moderatorProfile == null || !moderatorProfile.ReliefStationId.HasValue)
            {
                throw new Exception("Bạn không thuộc trạm nào");
            }

            return moderatorProfile.ReliefStationId.Value;
        }

        private async Task EnsureCanManageVehicleAsync(
            Vehicle vehicle,
            Guid userId,
            bool isManager,
            bool isModerator,
            CancellationToken cancellationToken)
        {
            if (isManager)
            {
                return;
            }

            if (isModerator)
            {
                var moderatorStationId = await GetModeratorStationIdAsync(userId, cancellationToken);
                if (vehicle.ReliefStationId != moderatorStationId)
                {
                    throw new Exception("Bạn chỉ được quản lý phương tiện trong trạm của mình");
                }

                return;
            }

            if (vehicle.CreatedBy != userId)
            {
                throw new Exception("Bạn không có quyền quản lý phương tiện này");
            }
        }

    }
}
