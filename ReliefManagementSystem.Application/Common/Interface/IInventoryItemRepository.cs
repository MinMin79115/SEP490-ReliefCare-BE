using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    /// <summary>
    /// Repository for InventoryItem entity
    /// </summary>
    public interface IInventoryItemRepository : IGenericRepository<InventoryItem>
    {
        Task<InventoryItem?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<InventoryItem>> GetAllWithCategoryAsync(CancellationToken cancellationToken = default);
        Task<List<InventoryItem>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
        Task<List<InventoryItem>> GetByStatusAsync(InventoryStatus status, CancellationToken cancellationToken = default);
        Task<InventoryItem?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
        Task<int> CountByStatusAsync(InventoryStatus status, CancellationToken cancellationToken = default);
        Task<int> CountCreatedTodayAsync(CancellationToken cancellationToken = default);
    }
}
