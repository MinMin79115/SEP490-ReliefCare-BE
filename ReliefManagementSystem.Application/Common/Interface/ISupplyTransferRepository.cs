using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ISupplyTransferRepository : IGenericRepository<SupplyTransfer>
    {
        Task<SupplyTransfer?> GetByIdWithDetailsAsync(Guid transferId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SupplyTransfer>> GetByStatusAsync(SupplyTransferStatus status, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SupplyTransfer>> GetBySourceStationAsync(Guid stationId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SupplyTransfer>> GetByDestinationStationAsync(Guid stationId, CancellationToken cancellationToken = default);
        Task AddVehicleAssignmentAsync(SupplyTransferVehicle assignment, CancellationToken cancellationToken = default);
        Task<int> CountTodayAsync(CancellationToken cancellationToken = default);
    }
}
