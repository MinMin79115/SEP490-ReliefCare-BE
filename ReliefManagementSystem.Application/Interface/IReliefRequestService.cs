using ReliefManagementSystem.Application.Features.ReliefRequest.DTOs.Request;
using ReliefManagementSystem.Application.Features.ReliefRequest.DTOs.Response;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IReliefRequestService
    {
        Task<ReliefRequestResponseDto> CreateReliefRequestAsync(CreateReliefRequestDto request, CancellationToken cancellationToken = default);
        Task<ReliefRequestResponseDto> GetReliefRequestByIdAsync(Guid requestId, CancellationToken cancellationToken = default);
        Task<PaginatedReliefRequestResponseDto> SearchReliefRequestsAsync(SearchReliefRequestDto request, CancellationToken cancellationToken = default);
        Task<ReliefRequestResponseDto> VerifyReliefRequestAsync(Guid requestId, VerifyReliefRequestDto dto, CancellationToken cancellationToken = default);
        Task<ReliefRequestResponseDto> ApproveReliefRequestAsync(Guid requestId, ApproveReliefRequestDto dto, CancellationToken cancellationToken = default);
        Task<ReliefRequestResponseDto> RejectReliefRequestAsync(Guid requestId, RejectReliefRequestDto dto, CancellationToken cancellationToken = default);
        Task<ReliefRequestResponseDto> AssignStationAsync(Guid requestId, AssignReliefRequestStationDto dto, CancellationToken cancellationToken = default);
        Task<ReliefRequestResponseDto> AssignCampaignAsync(Guid requestId, AssignReliefRequestCampaignDto dto, CancellationToken cancellationToken = default);
    }
}
