using Microsoft.AspNetCore.Identity;
using ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions;
using ReliefManagementSystem.Application.Common.Exceptions.Team;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.ReliefStation.Dtos;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Request;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    /// <summary>
    /// Business logic for ReliefStation and team assignment management.
    /// </summary>
    public class ReliefStationService : IReliefStationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReliefStationService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _userManager = userManager;
        }

        public async Task<Guid> CreateProvincialReliefStationAsync(CreateProvincialReliefStationRequest request, CancellationToken cancellationToken)
        {
           

            // 2️⃣ Validate coordinates
            if (request.Latitude < -90 || request.Latitude > 90 ||
                request.Longitude < -180 || request.Longitude > 180)
            {
                throw new InvalidCoordinatesException();
            }

            // 3️⃣ Check Location exists
            var location = await _unitOfWork.Locations.GetByIdAsync(request.LocationId);

            if (location == null)
            {
                throw new LocationNotFoundException();
            }

            // 4️⃣ Validate location level (optional nếu Location có Level)
            if (location.Level != LocationLevel.Province)
            {
                throw new InvalidLocationForProvincialStationException();
            }


            if (await _unitOfWork.ReliefStations.ExistsByNameAsync(request.Name))
            {
                throw new StationNameAlreadyExistsException(request.Name);
            }


            if (await _unitOfWork.ReliefStations.ExistsProvincialStationInLocationAsync(request.LocationId))
            {
                throw new DuplicateReliefStationLocationException();
            }

            try
            {

                var station = new ReliefStation
                {
                    LocationId = request.LocationId,
                    Name = request.Name,
                    Address = request.Address,
                    ContactNumber = request.ContactNumber,
                    Longitude = request.Longitude,
                    Latitude = request.Latitude,

                    Level = ReliefStationLevel.Provincial,
                    ReliefStationStatus = ReliefStationStatus.Active,
                };

                var inventory = new Inventory
                {
                    Level = InventoryLevel.Provincial,
                    Status = EntityStatus.Active,
                    ReliefStation = station

                };

                await _unitOfWork.ReliefStations.AddAsync(station);
                await _unitOfWork.Inventories.AddAsync(inventory);

                var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (result <= 0)
                {
                    throw new ReliefStationCreationFailedException();
                }

                return station.ReliefStationId;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new InventoryCreationFailedException();
            }
        }
        public async Task<ReliefStationResponse> UpdateProvincialReliefStationAsync(Guid stationId, UpdateProvincialStationRequest request, CancellationToken cancellationToken)
        {
            // 1️⃣ Lấy trạm cần cập nhật
            var station = await _unitOfWork.ReliefStations.GetByIdAsync(stationId);
            if (station == null)
            {
                throw new ReliefStationNotFoundException(stationId);
            }

            // 2️⃣ Kiểm tra đúng cấp Provincial
            if (station.Level != ReliefStationLevel.Provincial)
            {
                throw new InvalidLocationForProvincialStationException();
            }

            // 3️⃣ Kiểm tra toạ độ
            if (request.Latitude < -90 || request.Latitude > 90 ||
                request.Longitude < -180 || request.Longitude > 180)
            {
                throw new InvalidCoordinatesException();
            }

            // 4️⃣ Kiểm tra tên không bị trùng với trạm khác
            if (station.Name != request.Name &&
                await _unitOfWork.ReliefStations.ExistsByNameExcludingIdAsync(request.Name, stationId))
            {
                throw new StationNameAlreadyExistsException(request.Name);
            }

            // 5️⃣ Cập nhật thông tin
            station.Name = request.Name;
            station.Address = request.Address;
            station.ContactNumber = request.ContactNumber;
            station.Longitude = request.Longitude;
            station.Latitude = request.Latitude;

            _unitOfWork.ReliefStations.UpdateAsync(station);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 6️⃣ Trả về thông tin trạm sau khi cập nhật
            return new ReliefStationResponse
            {
                ReliefStationId = station.ReliefStationId,
                Name = station.Name,
                Address = station.Address,
                ContactNumber = station.ContactNumber,
                Longitude = station.Longitude,
                Latitude = station.Latitude,
                Status = station.ReliefStationStatus,
                Level = station.Level,
                LocationId = station.LocationId,
                LocationName = station.Location?.Name ?? string.Empty,
                UpdatedAt = station.UpdatedAt
            };
        }
        public async Task<(List<ReliefStationResponse> Items, int TotalCount)> GetProvincialStationsAsync(
            string? search, int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            var (stations, totalCount) = await _unitOfWork.ReliefStations
                .GetProvincialStationsAsync(search, pageIndex, pageSize, cancellationToken);

            var items = stations.Select(s => new ReliefStationResponse
            {
                ReliefStationId = s.ReliefStationId,
                Name = s.Name,
                Address = s.Address,
                ContactNumber = s.ContactNumber,
                Longitude = s.Longitude,
                Latitude = s.Latitude,
                Status = s.ReliefStationStatus,
                Level = s.Level,
                LocationId = s.LocationId,
                LocationName = s.Location?.Name ?? string.Empty,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            }).ToList();

            return (items, totalCount);
        }

        public async Task<ReliefStationResponse> GetCurrentModeratorStationAsync(CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId;
            if (!currentUserId.HasValue)
            {
                throw new ModeratorProfileNotFoundException();
            }
            var currentUser = await _unitOfWork.Users.GetUserById(currentUserId.Value);

            var moderatorProfile = await _unitOfWork.ModeratorProfiles
                .GetByUserIdAsync(currentUserId.Value, cancellationToken);

            var locationName = await _unitOfWork.Locations.GetFullNameByLocationId(moderatorProfile.ReliefStation.LocationId);
            if (moderatorProfile == null)
            {
                throw new ModeratorProfileNotFoundException(currentUserId.Value);
            }

            if (!moderatorProfile.ReliefStationId.HasValue || moderatorProfile.ReliefStation == null)
            {
                throw new ModeratorStationNotAssignedException();
            }

            var station = moderatorProfile.ReliefStation;

            return new ReliefStationResponse
            {
                ReliefStationId = station.ReliefStationId,
                Name = station.Name,
                ModeratorName = currentUser?.DisplayName ?? string.Empty,
                Address = station.Address,
                ContactNumber = station.ContactNumber,
                Longitude = station.Longitude,
                Latitude = station.Latitude,
                Status = station.ReliefStationStatus,
                Level = station.Level,
                LocationId = station.LocationId,
                LocationName = locationName,
                CreatedAt = station.CreatedAt,
                UpdatedAt = station.UpdatedAt
            };
        }

        public async Task<ReliefStationResponse> DisableProvincialStationAsync(Guid stationId, CancellationToken cancellationToken)
        {
            // 1️⃣ Lấy trạm cần disable
            var station = await _unitOfWork.ReliefStations.GetByIdAsync(stationId);
            if (station == null)
            {
                throw new ReliefStationNotFoundException(stationId);
            }

            // 2️⃣ Kiểm tra đúng cấp Provincial
            if (station.Level != ReliefStationLevel.Provincial)
            {
                throw new InvalidLocationForProvincialStationException();
            }

            // 3️⃣ Cập nhật trạng thái trạm
            station.ReliefStationStatus = ReliefStationStatus.Inactive;

            // 4️⃣ Cập nhật Data Inventory thuộc về trạm này
            var inventories = await _unitOfWork.Inventories.GetByReliefStationAsync(stationId, cancellationToken);
            foreach (var inventory in inventories)
            {
                inventory.Status = EntityStatus.Inactive;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5️⃣ Trả về thông tin trạm sau khi disable
            return new ReliefStationResponse
            {
                ReliefStationId = station.ReliefStationId,
                Name = station.Name,
                Address = station.Address,
                ContactNumber = station.ContactNumber,
                Longitude = station.Longitude,
                Latitude = station.Latitude,
                Status = station.ReliefStationStatus,
                Level = station.Level,
                LocationId = station.LocationId,
                LocationName = station.Location?.Name ?? string.Empty,
                CreatedAt = station.CreatedAt,
                UpdatedAt = station.UpdatedAt
            };
        }
        public async Task<ReliefStationResponse> ActivateProvincialStationAsync(Guid stationId, CancellationToken cancellationToken)
        {
            // 1️⃣ Lấy trạm cần activate
            var station = await _unitOfWork.ReliefStations.GetByIdAsync(stationId);
            if (station == null)
            {
                throw new ReliefStationNotFoundException(stationId);
            }

            // 2️⃣ Kiểm tra đúng cấp Provincial
            if (station.Level != ReliefStationLevel.Provincial)
            {
                throw new InvalidLocationForProvincialStationException();
            }

            // 3️⃣ Cập nhật trạng thái trạm
            station.ReliefStationStatus = ReliefStationStatus.Active;

            // 4️⃣ Cập nhật Data Inventory thuộc về trạm này
            // Lưu ý: với GetByReliefStationAsync hiện tại trong repository thường chỉ lấy Active, 
            // nên nếu repository đang lọc Active thì phải sửa hoặc thêm hàm Get All (kể cả inactive)
            var inventories = await _unitOfWork.Inventories.GetAllAsync();
            var stationInventories = inventories.Where(i => i.ReliefStationId == stationId).ToList();
            
            foreach (var inventory in stationInventories)
            {
                inventory.Status = EntityStatus.Active;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5️⃣ Trả về thông tin trạm sau khi activate
            return new ReliefStationResponse
            {
                ReliefStationId = station.ReliefStationId,
                Name = station.Name,
                Address = station.Address,
                ContactNumber = station.ContactNumber,
                Longitude = station.Longitude,
                Latitude = station.Latitude,
                Status = station.ReliefStationStatus,
                Level = station.Level,
                LocationId = station.LocationId,
                LocationName = station.Location?.Name ?? string.Empty,
                CreatedAt = station.CreatedAt,
                UpdatedAt = station.UpdatedAt
            };
        }

        public async Task AssignModeratorAsync(Guid stationId, AssignModeratorRequest request, CancellationToken cancellationToken)
        {
            // 1. Kiểm tra trạm tồn tại
            var station = await _unitOfWork.ReliefStations.GetByIdAsync(stationId);
            if (station == null)
            {
                throw new ReliefStationNotFoundException(stationId);
            }

            // 2. Tìm profile Moderator theo UserId
            var moderatorProfile = await _unitOfWork.ModeratorProfiles.GetByUserIdAsync(request.ModeratorUserId, cancellationToken);
            if (moderatorProfile == null)
            {
                throw new ModeratorProfileNotFoundException(request.ModeratorUserId);
            }

            // 3. Kiểm tra xem trạm đã có Moderator phụ trách (trưởng trạm) chưa
            // Vì yêu cầu: 1 trạm chỉ được có 1 moderator, nên moderator này sẽ là trưởng trạm
            var currentHead = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(stationId, cancellationToken);
            if (currentHead != null && currentHead.UserId != request.ModeratorUserId)
            {
                // Nếu đã có trưởng trạm và không phải là người đang được gán thì báo lỗi
                throw new StationAlreadyHasModeratorException(stationId);
            }

            // 4. Gán trạm cho Moderator
            moderatorProfile.ReliefStationId = stationId;
            moderatorProfile.IsStationHead = true; // 1 trạm 1 mod -> mặc định là head
            moderatorProfile.Status = request.Status ?? ModeratorStatus.Active;
            moderatorProfile.StatusReason = request.Reason;

            await _unitOfWork.ModeratorProfiles.UpdateAsync(moderatorProfile);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<StationTeamResponse> AssignTeamToStationAsync(Guid stationId, AssignTeamRequest request, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId;
            if (!currentUserId.HasValue)
            {
                throw new OnlyStationHeadCanManageAssignmentsException();
            }

            var station = await _unitOfWork.ReliefStations.GetByIdAsync(stationId);
            if (station == null)
            {
                throw new ReliefStationNotFoundException(stationId);
            }

            if (station.Level != ReliefStationLevel.Provincial || station.Level != ReliefStationLevel.Regional)
            {
                throw new InvalidLocationForProvincialStationException();
            }

            var stationHead = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(stationId, cancellationToken);
            if (stationHead == null || stationHead.UserId != currentUserId.Value)
            {
                throw new OnlyStationHeadCanManageAssignmentsException();
            }

            var team = await _unitOfWork.Teams.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                throw new TeamNotFoundException(request.TeamId);
            }

            if (team.Status != TeamStatus.Active)
            {
                throw new TeamInactiveException(team.Name);
            }

            var existing = await _unitOfWork.ReliefStationTeams
                .GetByStationAndTeamAsync(stationId, request.TeamId, cancellationToken);

            if (existing != null)
            {
                if (existing.Status == ReliefTeamAssignmentStatus.Pending)
                {
                    existing.Status = ReliefTeamAssignmentStatus.Approved;
                    existing.Description = request.Description ?? existing.Description;
                    existing.RejectionReason = null;
                    existing.JoinedAt ??= DateTime.UtcNow;
                    await _unitOfWork.ReliefStationTeams.UpdateAsync(existing);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    return new StationTeamResponse
                    {
                        AssignmentId = existing.ReliefStationTeamId,
                        TeamId = existing.TeamId,
                        TeamName = existing.Team?.Name ?? team.Name,
                        Status = existing.Status,
                        Description = existing.Description,
                        RejectionReason = existing.RejectionReason,
                        JoinedAt = existing.JoinedAt,
                    };
                }

                throw new ReliefStationAssignmentAlreadyExistsException(stationId, request.TeamId);
            }

            var assignment = new ReliefStationTeam
            {
                ReliefStationTeamId = Guid.NewGuid(),
                ReliefStationId = stationId,
                TeamId = request.TeamId,
                Status = ReliefTeamAssignmentStatus.Approved,
                Description = request.Description,
                JoinedAt = DateTime.UtcNow
            };

            await _unitOfWork.ReliefStationTeams.AddAsync(assignment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new StationTeamResponse
            {
                AssignmentId = assignment.ReliefStationTeamId,
                TeamId = assignment.TeamId,
                TeamName = team.Name,
                Status = assignment.Status,
                Description = assignment.Description,
                RejectionReason = assignment.RejectionReason,
                JoinedAt = assignment.JoinedAt,
            };
        }

        public async Task<StationTeamResponse> UpdateTeamAssignmentStatusAsync(
            Guid stationId,
            Guid teamId,
            UpdateTeamAssignmentRequest request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId;
            if (!currentUserId.HasValue)
            {
                throw new OnlyStationHeadCanManageAssignmentsException();
            }

            var stationHead = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(stationId, cancellationToken);
            if (stationHead == null || stationHead.UserId != currentUserId.Value)
            {
                throw new OnlyStationHeadCanManageAssignmentsException();
            }

            var assignment = await _unitOfWork.ReliefStationTeams
                .GetByStationAndTeamAsync(stationId, teamId, cancellationToken);

            if (assignment == null)
            {
                throw new ReliefStationAssignmentNotFoundException(stationId, teamId);
            }

            assignment.Status = request.Status;
            assignment.Description = request.Description ?? assignment.Description;

            if (request.Status == ReliefTeamAssignmentStatus.Rejected)
            {
                if (string.IsNullOrWhiteSpace(request.RejectionReason))
                {
                    throw new RejectionReasonRequiredException();
                }

                assignment.RejectionReason = request.RejectionReason;
            }
            else if (request.Status == ReliefTeamAssignmentStatus.Approved)
            {
                assignment.RejectionReason = null;
                assignment.JoinedAt ??= DateTime.UtcNow;
            }

            await _unitOfWork.ReliefStationTeams.UpdateAsync(assignment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new StationTeamResponse
            {
                AssignmentId = assignment.ReliefStationTeamId,
                TeamId = assignment.TeamId,
                TeamName = assignment.Team?.Name ?? string.Empty,
                Status = assignment.Status,
                Description = assignment.Description,
                RejectionReason = assignment.RejectionReason,
                JoinedAt = assignment.JoinedAt,
            };
        }

    }
}
