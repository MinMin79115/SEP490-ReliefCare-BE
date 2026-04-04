using ReliefManagementSystem.Application.Features.SupplyTransfer.DTOs.Request;
using ReliefManagementSystem.Application.Features.SupplyTransfer.DTOs.Response;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Interface
{
    public interface ISupplyTransferService
    {
        Task<SupplyTransferResponse> CreateAsync(CreateSupplyTransferRequest request, CancellationToken cancellationToken = default);
        Task<SupplyTransferResponse> GetByIdAsync(Guid transferId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SupplyTransferSummaryResponse>> GetByStatusAsync(SupplyTransferStatus status, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SupplyTransferSummaryResponse>> GetBySourceStationAsync(Guid stationId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SupplyTransferSummaryResponse>> GetByDestinationStationAsync(Guid stationId, CancellationToken cancellationToken = default);
        Task<SupplyTransferResponse> ApproveAsync(Guid transferId, ApproveSupplyTransferRequest request, CancellationToken cancellationToken = default);
        Task<SupplyTransferResponse> ShipAsync(Guid transferId, ShipSupplyTransferRequest request, CancellationToken cancellationToken = default);
        Task<SupplyTransferResponse> ReceiveAsync(Guid transferId, ReceiveSupplyTransferRequest request, CancellationToken cancellationToken = default);
        Task<SupplyTransferResponse> CancelAsync(Guid transferId, CancelSupplyTransferRequest request, CancellationToken cancellationToken = default);
    }
}
