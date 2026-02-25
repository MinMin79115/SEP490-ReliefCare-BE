using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    /// <summary>
    /// Concrete implementation of <see cref="IInventoryRepository"/>.
    /// </summary>
    public class InventoryRepository : GenericRepository<Inventory>, IInventoryRepository
    {
        public InventoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Inventory>> GetAllActiveAsync(
            Guid? reliefStationId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(i => i.Status != EntityStatus.Deleted);

            if (reliefStationId.HasValue)
            {
                query = query.Where(i => i.ReliefStationId == reliefStationId.Value);
            }

            return await query
                .Include(i => i.ReliefStation)
                .OrderBy(i => i.Level)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<Inventory?> GetByIdWithStocksAsync(
            Guid inventoryId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(i => i.ReliefStation)
                .Include(i => i.InventoryItems)
                    .ThenInclude(s => s.SupplyItem)
                .FirstOrDefaultAsync(i => i.InventoryId == inventoryId && i.Status != EntityStatus.Deleted, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Inventory>> GetByReliefStationAsync(
            Guid reliefStationId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(i => i.ReliefStationId == reliefStationId && i.Status != EntityStatus.Deleted)
                .Include(i => i.ReliefStation)
                .OrderBy(i => i.Level)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> IsLevelExistsForStationAsync(
            Guid reliefStationId,
            InventoryLevel level,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(i =>
                i.ReliefStationId == reliefStationId &&
                i.Level == level &&
                i.Status != EntityStatus.Deleted);

            if (excludeId.HasValue)
            {
                query = query.Where(i => i.InventoryId != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }
    }
}
