using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    /// <summary>
    /// Repository for WarehouseTransaction entity
    /// </summary>
    public interface IWarehouseTransactionRepository : IGenericRepository<WarehouseTransaction>
    {
        Task<List<WarehouseTransaction>> GetByBatchIdAsync(Guid batchId, CancellationToken cancellationToken = default);
        Task<List<WarehouseTransaction>> GetByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default);
        Task BulkAddAsync(List<WarehouseTransaction> transactions, CancellationToken cancellationToken = default);
    }
}
