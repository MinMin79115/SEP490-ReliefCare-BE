using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    /// <summary>
    /// Concrete implementation of <see cref="IInventoryTransactionRepository"/>.
    /// </summary>
    public class InventoryTransactionRepository : GenericRepository<InventoryTransaction>, IInventoryTransactionRepository
    {
        public InventoryTransactionRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<InventoryTransaction>> GetByInventoryIdAsync(
            Guid inventoryId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(t => t.InventoryId == inventoryId)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Items)
                    .ThenInclude(i => i.SupplyItem)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<InventoryTransaction?> GetByIdWithItemsAsync(
            Guid transactionId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(t => t.Inventory)
                    .ThenInclude(i => i.ReliefStation)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Items)
                    .ThenInclude(i => i.SupplyItem)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<InventoryTransaction>> GetByTypeAsync(
            TransactionType type,
            Guid? inventoryId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(t => t.Type == type);

            if (inventoryId.HasValue)
                query = query.Where(t => t.InventoryId == inventoryId.Value);

            return await query
                .Include(t => t.CreatedByUser)
                .Include(t => t.Items)
                    .ThenInclude(i => i.SupplyItem)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<int> CountTodayByTypeAsync(
            TransactionType type,
            CancellationToken cancellationToken = default)
        {
            var todayUtc = DateTime.UtcNow.Date;
            return await _dbSet
                .CountAsync(t => t.Type == type && t.CreatedAt >= todayUtc, cancellationToken);
        }
        /// <inheritdoc/>
        public IQueryable<InventoryTransaction> GetQueryable()
        {
            return _dbSet
                .Include(t => t.CreatedByUser)
                .Include(t => t.Items)
                    .ThenInclude(i => i.SupplyItem);
        }
    }
}
