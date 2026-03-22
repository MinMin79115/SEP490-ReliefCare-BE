using ReliefManagementSystem.Application.Features.Procurement.Dtos.Requests;
using ReliefManagementSystem.Application.Features.Procurement.Dtos.Responses;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IProcurementService
    {
        Task<ProcurementOrderResponse> CreateAsync(CreateProcurementOrderRequest request, CancellationToken cancellationToken = default);
        Task<ProcurementOrderResponse> GetByIdAsync(Guid procurementOrderId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ProcurementOrderResponse>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<ProcurementOrderResponse> ApproveAsync(Guid procurementOrderId, ApproveProcurementOrderRequest request, CancellationToken cancellationToken = default);
        Task<ProcurementOrderResponse> ReceiveAsync(Guid procurementOrderId, ReceiveProcurementOrderRequest request, CancellationToken cancellationToken = default);
        Task<ProcurementOrderResponse> CancelAsync(Guid procurementOrderId, CancellationToken cancellationToken = default);
    }
}
