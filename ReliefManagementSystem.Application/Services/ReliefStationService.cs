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

        public ReliefStationService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
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
