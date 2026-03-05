using Microsoft.AspNetCore.Identity;
using ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
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

        // ═══════════════════════════════════════════════════════════
        //  GET ALL STATIONS (PAGINATED)
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<Pagination<ReliefStationResponse>> GetAllStationsAsync(
            GetAllStationsRequest request,
            CancellationToken ct = default)
        {
            var query = _unitOfWork.ReliefStations
                .GetAllQueryable(request.Level, request.Search);

            // Phân trang trên entity
            var pagedStations = await Pagination<ReliefStation>.ToPagedList(
                query, request.PageIndex, request.PageSize);

            // Map sang response
            var responses = pagedStations.Items!
                .Select(rs => MapToResponse(rs, rs.Location?.Name ?? string.Empty))
                .ToList();

            return new Pagination<ReliefStationResponse>(
                responses,
                pagedStations.TotalCount,
                pagedStations.CurrentPage,
                pagedStations.PageSize);
        }

        // ═══════════════════════════════════════════════════════════
        //  CREATE PROVINCIAL STATION
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<ReliefStationResponse> CreateProvincialStationAsync(
            CreateProvincialStationRequest request,
            CancellationToken ct = default)
        {
            // 1. Kiểm tra user hiện tại có ManagerProfile không
            var managerProfile = await _unitOfWork.ManagerProfiles
                .GetByUserIdAsync(_currentUser.UserId, ct);

            if (managerProfile is null)
                throw new UnauthorizedStationCreationException();

            // 2. Kiểm tra LocationId có tồn tại không
            var location = await _unitOfWork.Locations.GetByIdAsync(request.LocationId);
            if (location is null)
                throw new LocationNotFoundException();

            // 3. Kiểm tra LocationId phải đúng cấp Tỉnh (Province)
            if (location.Level != LocationLevel.Province)
                throw new InvalidLocationLevelException("Tỉnh (Province)");

            // 4. Tìm trạm Regional cha trong vùng Manager phụ trách
            //    AssignedLocationId của Manager phải là LocationId cấp Region
            if (managerProfile.AssignedLocationId is null)
                throw new ParentStationNotFoundException();

            var parentStation = await _unitOfWork.ReliefStations
                .GetRegionalByLocationIdAsync(managerProfile.AssignedLocationId.Value, ct);

            if (parentStation is null)
                throw new ParentStationNotFoundException();

            // 5. Tạo trạm tỉnh
            var now = DateTime.UtcNow;
            var station = new ReliefStation
            {
                ReliefStationId = Guid.NewGuid(),
                Name             = request.Name,
                LocationId       = request.LocationId,
                Address          = request.Address,
                ContactNumber    = request.ContactNumber,
                Longitude        = request.Longitude,
                Latitude         = request.Latitude,
                Level            = ReliefStationLevel.Provincial,
                ParentReliefStationId = parentStation.ReliefStationId,
                Status           = ReliefStationStatus.Draft,
                IsActive         = true,
                CreatedBy        = _currentUser.UserId,
                CreatedAt        = now,
                UpdatedAt        = now
            };

            await _unitOfWork.ReliefStations.AddAsync(station);
            await _unitOfWork.SaveChangesAsync(ct);

            return MapToResponse(station, location.Name);
        }

        // ═══════════════════════════════════════════════════════════
        //  CREATE LOCAL STATION
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<ReliefStationResponse> CreateLocalStationAsync(
            CreateLocalStationRequest request,
            CancellationToken ct = default)
        {
            // 1. Kiểm tra user hiện tại có ModeratorProfile + IsStationHead không
            var moderatorProfile = await _unitOfWork.ModeratorProfiles
                .GetByUserIdAsync(_currentUser.UserId, ct);

            if (moderatorProfile is null || !moderatorProfile.IsStationHead)
                throw new UnauthorizedStationCreationException();

            // 2. Trạm mà Moderator đứng đầu phải là trạm Provincial
            if (moderatorProfile.ReliefStationId is null)
                throw new ParentStationNotFoundException();

            var parentStation = await _unitOfWork.ReliefStations
                .GetByIdAsync(moderatorProfile.ReliefStationId.Value);

            if (parentStation is null || parentStation.Level != ReliefStationLevel.Provincial)
                throw new ParentStationNotFoundException();

            // 3. Kiểm tra LocationId có tồn tại không
            var location = await _unitOfWork.Locations.GetByIdAsync(request.LocationId);
            if (location is null)
                throw new LocationNotFoundException();

            // 4. Kiểm tra LocationId phải đúng cấp Xã/Phường (Commune)
            if (location.Level != LocationLevel.Commune)
                throw new InvalidLocationLevelException("Xã/Phường (Commune)");

            // 5. Tạo trạm địa phương
            var now = DateTime.UtcNow;
            var station = new ReliefStation
            {
                ReliefStationId       = Guid.NewGuid(),
                Name                  = request.Name,
                LocationId            = request.LocationId,
                Address               = request.Address,
                ContactNumber         = request.ContactNumber,
                Longitude             = request.Longitude,
                Latitude              = request.Latitude,
                Level                 = ReliefStationLevel.Local,
                ParentReliefStationId = parentStation.ReliefStationId,
                Status                = ReliefStationStatus.Draft,
                IsActive              = true,
                CreatedBy             = _currentUser.UserId,
                CreatedAt             = now,
                UpdatedAt             = now
            };

            await _unitOfWork.ReliefStations.AddAsync(station);
            await _unitOfWork.SaveChangesAsync(ct);

            return MapToResponse(station, location.Name);
        }

        // ═══════════════════════════════════════════════════════════
        //  ASSIGN MODERATOR
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<bool> AssignModeratorAsync(
            Guid stationId,
            UpdateTeamAssignmentRequest.AssignModeratorRequest request,
            CancellationToken ct = default)
        {
            // 1. Get Station
            var station = await _unitOfWork.ReliefStations.GetByIdAsync(stationId);
            if (station is null)
                throw new ReliefStationNotFoundException();

            // 2. Authorization
            var user = await _userManager.FindByIdAsync(_currentUser.UserId.ToString());
            if (user == null)
                throw new UnauthorizedModeratorAssignmentException();

            var roles = await _userManager.GetRolesAsync(user);
            bool isAuthorized = false;

            if (roles.Contains(Role.Admin.ToString()))
            {
                isAuthorized = true;
            }
            else if (roles.Contains(Role.Manager.ToString()))
            {
                var managerProfile = await _unitOfWork.ManagerProfiles.GetByUserIdAsync(_currentUser.UserId, ct);
                if (managerProfile?.AssignedLocationId != null)
                {
                    var regionalStation = await _unitOfWork.ReliefStations
                        .GetRegionalByLocationIdAsync(managerProfile.AssignedLocationId.Value, ct);

                    if (regionalStation != null)
                    {
                        if (station.ReliefStationId == regionalStation.ReliefStationId)
                            isAuthorized = true;
                        else if (station.ParentReliefStationId == regionalStation.ReliefStationId)
                            isAuthorized = true;
                        else if (station.Level == ReliefStationLevel.Local)
                        {
                            var parentStation = await _unitOfWork.ReliefStations.GetByIdAsync(station.ParentReliefStationId ?? Guid.Empty);
                            if (parentStation?.ParentReliefStationId == regionalStation.ReliefStationId)
                                isAuthorized = true;
                        }
                    }
                }
            }
            else if (roles.Contains(Role.Moderator.ToString()))
            {
                var modProfile = await _unitOfWork.ModeratorProfiles.GetByUserIdAsync(_currentUser.UserId, ct);
                if (modProfile != null && modProfile.IsStationHead && modProfile.ReliefStationId == station.ParentReliefStationId)
                {
                    isAuthorized = true;
                }
            }

            if (!isAuthorized)
                throw new UnauthorizedModeratorAssignmentException();

            // 3. Find Moderator Profile
            var targetMod = await _unitOfWork.ModeratorProfiles.GetByUserIdAsync(request.ModeratorUserId, ct);
            if (targetMod == null)
                throw new ModeratorProfileNotFoundException();

            if (targetMod.ReliefStationId != null && targetMod.ReliefStationId != stationId)
            {
                // Cho phép gỡ khỏi trạm hiện tại nếu trạng thái truyền vào mang ý nghĩa ngắt/thôi việc
                // Thay vì cấm hoàn toàn, nếu đổi Status = Inactive/Suspended/Dismissed thì cho phép gỡ ReliefStationId
                if (request.Status is ModeratorStatus.Inactive or ModeratorStatus.Suspended or ModeratorStatus.Dismissed)
                {
                   targetMod.ReliefStationId = null;
                   targetMod.IsStationHead = false;
                }
                else
                {
                   throw new ModeratorAlreadyAssignedException();
                }
            }

            // 4. Handle IsStationHead logic (chỉ khi đang Active và gán trạm)
            if (request.IsStationHead && (request.Status == null || request.Status == ModeratorStatus.Active))
            {
                var existingHead = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(stationId, ct);
                if (existingHead != null && existingHead.UserId != targetMod.UserId)
                {
                    existingHead.IsStationHead = false;
                    _unitOfWork.ModeratorProfiles.UpdateAsync(existingHead);
                }
            }

            // 5. Update Status & Reason
            if (request.Status.HasValue)
            {
                targetMod.Status = request.Status.Value;
                targetMod.StatusReason = request.Reason;
                
                // Nếu bị đình chỉ/sa thải/không hoạt động -> Gỡ khỏi trạm
                if (targetMod.Status != ModeratorStatus.Active)
                {
                    targetMod.ReliefStationId = null;
                    targetMod.IsStationHead = false;
                }
                else
                {
                    targetMod.ReliefStationId = stationId;
                    targetMod.IsStationHead = request.IsStationHead;
                }
            }
            else // Mặc định gán vào trạm -> Active
            {
                targetMod.ReliefStationId = stationId;
                targetMod.IsStationHead = request.IsStationHead;
                targetMod.Status = ModeratorStatus.Active;
                targetMod.StatusReason = request.Reason;
            }

            _unitOfWork.ModeratorProfiles.UpdateAsync(targetMod);
            await _unitOfWork.SaveChangesAsync(ct);

            return true;
        }

        // ═══════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ═══════════════════════════════════════════════════════════

        private static ReliefStationResponse MapToResponse(ReliefStation rs, string locationName) => new()
        {
            ReliefStationId       = rs.ReliefStationId,
            Name                  = rs.Name,
            Address               = rs.Address,
            ContactNumber         = rs.ContactNumber,
            Longitude             = rs.Longitude,
            Latitude              = rs.Latitude,
            Status                = rs.Status,
            IsActive              = rs.IsActive,
            Level                 = rs.Level,
            ParentReliefStationId = rs.ParentReliefStationId,
            LocationId            = rs.LocationId,
            LocationName          = locationName,
            CreatedAt             = rs.CreatedAt,
            UpdatedAt             = rs.UpdatedAt
        };
    }
}
