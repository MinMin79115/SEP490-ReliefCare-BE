using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Request;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
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

        // ═══════════════════════════════════════════════════
        //  ReliefStation CRUD
        // ═══════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<ReliefStationResponse> CreateAsync(
            CreateReliefStationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (await _unitOfWork.ReliefStations.IsNameExistsAsync(request.Name, cancellationToken: cancellationToken))
                throw new InvalidOperationException($"A relief station with name '{request.Name}' already exists.");

            var station = new Domain.Entities.ReliefStation
            {
                ReliefStationId = Guid.NewGuid(),
                Name = request.Name,
                LocationId = request.LocationId,
                ManagerId = request.ManagerId,
                Address = request.Address,
                ContactNumber = request.ContactNumber,
                Longitude = request.Longitude,
                Latitude = request.Latitude,
                Status = request.Status,
                IsActive = request.Status == RelifeStationStatus.Active,
                CreatedBy = _currentUser.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ReliefStations.AddAsync(station);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with navigation for response
            var created = await _unitOfWork.ReliefStations.GetByIdWithDetailsAsync(station.ReliefStationId, cancellationToken);
            return MapToResponse(created!);
        }

        /// <inheritdoc/>
        public async Task<ReliefStationDetailResponse> GetByIdAsync(
            Guid stationId,
            CancellationToken cancellationToken = default)
        {
            var station = await _unitOfWork.ReliefStations.GetByIdWithDetailsAsync(stationId, cancellationToken);
            if (station is null)
                throw new KeyNotFoundException($"Relief station '{stationId}' was not found.");

            return MapToDetailResponse(station);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ReliefStationResponse>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var stations = await _unitOfWork.ReliefStations.GetAllWithDetailsAsync(cancellationToken);
            return stations.Select(MapToResponse).ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ReliefStationResponse>> GetByStatusAsync(
            RelifeStationStatus status,
            CancellationToken cancellationToken = default)
        {
            var stations = await _unitOfWork.ReliefStations.GetByStatusAsync(status, cancellationToken);
            return stations.Select(MapToResponse).ToList();
        }

        /// <inheritdoc/>
        public async Task<ReliefStationResponse> UpdateAsync(
            Guid stationId,
            UpdateReliefStationRequest request,
            CancellationToken cancellationToken = default)
        {
            var station = await _unitOfWork.ReliefStations.GetByIdAsync(stationId);
            if (station is null)
                throw new KeyNotFoundException($"Relief station '{stationId}' was not found.");

            // Check name uniqueness (exclude self)
            if (await _unitOfWork.ReliefStations.IsNameExistsAsync(request.Name, stationId, cancellationToken))
                throw new InvalidOperationException($"A relief station with name '{request.Name}' already exists.");

            station.Name = request.Name;
            station.LocationId = request.LocationId;
            station.ManagerId = request.ManagerId;
            station.Address = request.Address;
            station.ContactNumber = request.ContactNumber;
            station.Longitude = request.Longitude;
            station.Latitude = request.Latitude;
            station.Status = request.Status;
            station.IsActive = request.Status == RelifeStationStatus.Active;
            station.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ReliefStations.UpdateAsync(station);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updated = await _unitOfWork.ReliefStations.GetByIdWithDetailsAsync(stationId, cancellationToken);
            return MapToResponse(updated!);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid stationId, CancellationToken cancellationToken = default)
        {
            var station = await _unitOfWork.ReliefStations.GetByIdAsync(stationId);
            if (station is null)
                throw new KeyNotFoundException($"Relief station '{stationId}' was not found.");

            // Soft-close instead of hard delete
            station.Status = RelifeStationStatus.Closed;
            station.IsActive = false;
            station.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.ReliefStations.UpdateAsync(station);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // ═══════════════════════════════════════════════════
        //  Team Assignment
        // ═══════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<StationTeamResponse> AssignTeamAsync(
            Guid stationId,
            AssignTeamRequest request,
            CancellationToken cancellationToken = default)
        {
            // Validate station exists
            var station = await _unitOfWork.ReliefStations.GetByIdAsync(stationId);
            if (station is null)
                throw new KeyNotFoundException($"Relief station '{stationId}' was not found.");

            // Validate team exists
            var team = await _unitOfWork.Teams.GetByIdAsync(request.TeamId);
            if (team is null)
                throw new KeyNotFoundException($"Team '{request.TeamId}' was not found.");

            // Check unique assignment
            if (await _unitOfWork.ReliefStationTeams.IsTeamAssignedAsync(stationId, request.TeamId, cancellationToken))
                throw new InvalidOperationException($"Team '{team.Name}' is already assigned to this station.");

            var assignment = new Domain.Entities.ReliefStationTeam
            {
                RelifeStationTeamId = Guid.NewGuid(),
                ReliefStationId = stationId,
                TeamId = request.TeamId,
                Status = ReliefTeamAssignmentStatus.Active,
                IsActive = true
            };

            await _unitOfWork.ReliefStationTeams.AddAsync(assignment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var saved = await _unitOfWork.ReliefStationTeams.GetByIdWithDetailsAsync(assignment.RelifeStationTeamId, cancellationToken);
            return MapToTeamResponse(saved!);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<StationTeamResponse>> GetTeamsAsync(
            Guid stationId,
            CancellationToken cancellationToken = default)
        {
            if (!await _unitOfWork.ReliefStations.ExistsAsync(stationId))
                throw new KeyNotFoundException($"Relief station '{stationId}' was not found.");

            var assignments = await _unitOfWork.ReliefStationTeams.GetByStationIdAsync(stationId, cancellationToken);
            return assignments.Select(MapToTeamResponse).ToList();
        }

        /// <inheritdoc/>
        public async Task<StationTeamResponse> UpdateTeamAssignmentAsync(
            Guid assignmentId,
            UpdateTeamAssignmentRequest request,
            CancellationToken cancellationToken = default)
        {
            var assignment = await _unitOfWork.ReliefStationTeams.GetByIdWithDetailsAsync(assignmentId, cancellationToken);
            if (assignment is null)
                throw new KeyNotFoundException($"Team assignment '{assignmentId}' was not found.");

            assignment.Status = request.Status;
            assignment.IsActive = request.Status == ReliefTeamAssignmentStatus.Active;

            await _unitOfWork.ReliefStationTeams.UpdateAsync(assignment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToTeamResponse(assignment);
        }

        /// <inheritdoc/>
        public async Task RemoveTeamAsync(Guid assignmentId, CancellationToken cancellationToken = default)
        {
            var assignment = await _unitOfWork.ReliefStationTeams.GetByIdAsync(assignmentId);
            if (assignment is null)
                throw new KeyNotFoundException($"Team assignment '{assignmentId}' was not found.");

            await _unitOfWork.ReliefStationTeams.DeleteAsync(assignment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // ═══════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ═══════════════════════════════════════════════════

        private static ReliefStationResponse MapToResponse(Domain.Entities.ReliefStation s) => new()
        {
            ReliefStationId = s.ReliefStationId,
            Name = s.Name,
            Address = s.Address,
            ContactNumber = s.ContactNumber,
            Longitude = s.Longitude,
            Latitude = s.Latitude,
            Status = s.Status,
            IsActive = s.IsActive,
            ManagerId = s.ManagerId,
            ManagerName = s.Manager?.DisplayName ?? string.Empty,
            LocationId = s.LocationId,
            LocationName = s.Location?.Name ?? string.Empty,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };

        private static ReliefStationDetailResponse MapToDetailResponse(Domain.Entities.ReliefStation s) => new()
        {
            ReliefStationId = s.ReliefStationId,
            Name = s.Name,
            Address = s.Address,
            ContactNumber = s.ContactNumber,
            Longitude = s.Longitude,
            Latitude = s.Latitude,
            Status = s.Status,
            IsActive = s.IsActive,
            ManagerId = s.ManagerId,
            ManagerName = s.Manager?.DisplayName ?? string.Empty,
            LocationId = s.LocationId,
            LocationName = s.Location?.Name ?? string.Empty,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            TotalInventories = s.Inventories?.Count ?? 0,
            Teams = s.ReliefStations?.Select(MapToTeamResponse).ToList() ?? []
        };

        private static StationTeamResponse MapToTeamResponse(Domain.Entities.ReliefStationTeam rst) => new()
        {
            AssignmentId = rst.RelifeStationTeamId,
            TeamId = rst.TeamId,
            TeamName = rst.Team?.Name ?? string.Empty,
            Status = rst.Status,
            IsActive = rst.IsActive
        };
    }
}
