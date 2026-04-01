using ReliefManagementSystem.Application.Features.DistributionSession.DTOs.Request;
using ReliefManagementSystem.Application.Features.DistributionSession.DTOs.Response;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IDistributionSessionService
    {
        Task<DistributionSessionResponseDto> CreateAsync(CreateDistributionSessionRequest request, CancellationToken cancellationToken = default);
        Task<DistributionSessionResponseDto> GetByIdAsync(Guid distributionSessionId, CancellationToken cancellationToken = default);
        Task<PaginatedDistributionSessionResponseDto> SearchAsync(SearchDistributionSessionRequest request, CancellationToken cancellationToken = default);
        Task<DistributionSessionResponseDto> AddItemsAsync(Guid distributionSessionId, AddDistributionSessionItemsRequest request, CancellationToken cancellationToken = default);
        Task<DistributionSessionResponseDto> AttachRequestsAsync(Guid distributionSessionId, AttachRequestsToSessionRequest request, CancellationToken cancellationToken = default);
        Task<DistributionSessionResponseDto> MarkReadyAsync(Guid distributionSessionId, CancellationToken cancellationToken = default);
        Task<DistributionSessionResponseDto> StartAsync(Guid distributionSessionId, CancellationToken cancellationToken = default);
        Task<DistributionSessionResponseDto> CompleteAsync(Guid distributionSessionId, CancellationToken cancellationToken = default);
        Task<DistributionSessionResponseDto> CancelAsync(Guid distributionSessionId, CancellationToken cancellationToken = default);
    }
}
