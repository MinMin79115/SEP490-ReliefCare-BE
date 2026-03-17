using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.ReliefStation.Dtos;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Request;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Response;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Interface
{
    /// <summary>Service contract for ReliefStation and team assignment operations.</summary>
    public interface IReliefStationService
    {
        Task<Guid> CreateProvincialReliefStationAsync(CreateProvincialReliefStationRequest request, CancellationToken cancellationToken);

        Task<ReliefStationResponse> UpdateProvincialReliefStationAsync(Guid stationId, UpdateProvincialStationRequest request, CancellationToken cancellationToken);

        Task<(List<ReliefStationResponse> Items, int TotalCount)> GetProvincialStationsAsync(
            string? search, int pageIndex, int pageSize, CancellationToken cancellationToken);

        Task<ReliefStationResponse> DisableProvincialStationAsync(Guid stationId, CancellationToken cancellationToken);
        
        Task<ReliefStationResponse> ActivateProvincialStationAsync(Guid stationId, CancellationToken cancellationToken);

        Task AssignModeratorAsync(Guid stationId, AssignModeratorRequest request, CancellationToken cancellationToken);

        Task<StationTeamResponse> AssignTeamToStationAsync(Guid stationId, AssignTeamRequest request, CancellationToken cancellationToken);

        Task<StationTeamResponse> UpdateTeamAssignmentStatusAsync(Guid stationId, Guid teamId, UpdateTeamAssignmentRequest request, CancellationToken cancellationToken);
    }
}
