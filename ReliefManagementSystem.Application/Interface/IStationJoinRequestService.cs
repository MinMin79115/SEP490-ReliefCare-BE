using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.StationJoinRequest.DTOs.Request;
using ReliefManagementSystem.Application.Features.StationJoinRequest.DTOs.Response;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IStationJoinRequestService
    {
        Task<StationJoinRequestResponse> CreateRequestAsync(CreateStationJoinRequestRequest request, Guid leaderId, CancellationToken cancellationToken);
        Task<StationJoinRequestResponse> GetByIdAsync(Guid requestId, CancellationToken cancellationToken);
        Task<Pagination<StationJoinRequestResponse>> GetMyRequestsAsync(Guid leaderId, int pageIndex, int pageSize, CancellationToken cancellationToken);
        Task<Pagination<StationJoinRequestResponse>> GetPendingByStationAsync(Guid stationId, Guid moderatorId, int pageIndex, int pageSize, CancellationToken cancellationToken);
        Task<StationJoinRequestResponse> ApproveAsync(Guid requestId, Guid moderatorId, ReviewStationJoinRequestRequest request, CancellationToken cancellationToken);
        Task<StationJoinRequestResponse> RejectAsync(Guid requestId, Guid moderatorId, ReviewStationJoinRequestRequest request, CancellationToken cancellationToken);
        Task<bool> CancelAsync(Guid requestId, Guid leaderId, CancellationToken cancellationToken);
    }
}
