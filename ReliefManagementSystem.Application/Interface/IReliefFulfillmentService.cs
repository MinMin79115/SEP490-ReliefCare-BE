using ReliefManagementSystem.Application.Features.ReliefFulfillment.DTOs.Request;
using ReliefManagementSystem.Application.Features.ReliefFulfillment.DTOs.Response;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IReliefFulfillmentService
    {
        Task<ReliefFulfillmentResponseDto> CreateAsync(Guid distributionSessionId, CreateReliefFulfillmentRequest request, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ReliefFulfillmentResponseDto>> GetByRequestAsync(Guid reliefRequestId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ReliefFulfillmentResponseDto>> GetBySessionAsync(Guid distributionSessionId, CancellationToken cancellationToken = default);
        Task<ReliefFulfillmentResponseDto> AddProofAsync(Guid reliefFulfillmentId, UpdateReliefFulfillmentProofRequest request, CancellationToken cancellationToken = default);
        Task<ReliefFulfillmentResponseDto> MarkFailedAsync(Guid reliefFulfillmentId, MarkReliefFulfillmentFailedRequest request, CancellationToken cancellationToken = default);
    }
}
