using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    /// <summary>
    /// Repository interface for SupplyItem master data operations.
    /// </summary>
    public interface ISupplyItemRepository : IGenericRepository<SupplyItem>
    {
        /// <summary>
        /// Gets all supply items, optionally filtered by category.
        /// </summary>
        Task<IReadOnlyList<SupplyItem>> GetAllAsync(SupplyCategory? category = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a supply item by name (case-insensitive).
        /// </summary>
        Task<SupplyItem?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether a supply item name already exists, optionally excluding a specific item.
        /// </summary>
        Task<bool> IsNameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a queryable for composing server-side filter + pagination.
        /// </summary>
        IQueryable<SupplyItem> GetQueryable();
    }
}
