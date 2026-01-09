namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IInventoryTransactionRepository : IGenericRepository<Domain.Entities.InventoryTransaction>
    {
        Task<Domain.Entities.InventoryTransaction?> GetByIdWithItemsAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<List<Domain.Entities.InventoryTransaction>> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default);

        Task<List<Domain.Entities.InventoryTransaction>> GetByTypeAsync(
            Domain.Enum.TransactionType type,
            CancellationToken cancellationToken = default);
    }
}
