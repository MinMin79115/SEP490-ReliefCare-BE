using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    /// <summary>
    /// Concrete implementation of <see cref="IInventoryStockRepository"/>.
    /// </summary>
    public class InventoryStockRepository : GenericRepository<InventoryStock>, IInventoryStockRepository
    {
        public InventoryStockRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<InventoryStock>> GetByInventoryIdAsync(
            Guid inventoryId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.InventoryId == inventoryId)
                .Include(s => s.SupplyItem)
                .OrderBy(s => s.SupplyItem.Category)
                .ThenBy(s => s.SupplyItem.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<InventoryStock>> GetByInventoryIdForUpdateAsync(
            Guid inventoryId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.InventoryId == inventoryId)
                .OrderBy(s => s.SupplyItemId)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<InventoryStock?> GetByInventoryAndSupplyItemAsync(
            Guid inventoryId,
            Guid supplyItemId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(s => s.SupplyItem)
                .FirstOrDefaultAsync(s =>
                    s.InventoryId == inventoryId &&
                    s.SupplyItemId == supplyItemId, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<InventoryStock?> GetByIdWithDetailsAsync(
            Guid stockId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(s => s.Inventory)
                    .ThenInclude(i => i.ReliefStation)
                .Include(s => s.SupplyItem)
                .FirstOrDefaultAsync(s => s.InventoryStockId == stockId, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> IsSupplyItemExistsInInventoryAsync(
            Guid inventoryId,
            Guid supplyItemId,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(s =>
                s.InventoryId == inventoryId &&
                s.SupplyItemId == supplyItemId);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.InventoryStockId != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public IQueryable<InventoryStock> GetQueryable()
        {
            return _dbSet
                .Include(s => s.SupplyItem);
        }
    }
}
