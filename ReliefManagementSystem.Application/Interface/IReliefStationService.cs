using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Request;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Response;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Interface
{
    /// <summary>Service contract for ReliefStation and team assignment operations.</summary>
    public interface IReliefStationService
    {
        // ── ReliefStation CRUD ──────────────────────────────────
        Task<ReliefStationResponse> CreateAsync(CreateReliefStationRequest request, CancellationToken cancellationToken = default);
        Task<ReliefStationDetailResponse> GetByIdAsync(Guid stationId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ReliefStationResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ReliefStationResponse>> GetByStatusAsync(ReliefStationStatus status, CancellationToken cancellationToken = default);
        Task<ReliefStationResponse> UpdateAsync(Guid stationId, UpdateReliefStationRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid stationId, CancellationToken cancellationToken = default);

        // ── Team Assignment ─────────────────────────────────────
        Task<StationTeamResponse> AssignTeamAsync(Guid stationId, AssignTeamRequest request, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StationTeamResponse>> GetTeamsAsync(Guid stationId, CancellationToken cancellationToken = default);
        Task<StationTeamResponse> UpdateTeamAssignmentAsync(Guid assignmentId, UpdateTeamAssignmentRequest request, CancellationToken cancellationToken = default);
        Task RemoveTeamAsync(Guid assignmentId, CancellationToken cancellationToken = default);
    }
}
