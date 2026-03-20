using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    /// <summary>
    /// Concrete implementation of <see cref="ISupplyAllocationRepository"/>.
    /// </summary>
    public class SupplyAllocationRepository : GenericRepository<SupplyAllocation>, ISupplyAllocationRepository
    {
        public SupplyAllocationRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<SupplyAllocation>> GetByCampaignIdAsync(
            Guid campaignId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(a => a.CampaignId == campaignId)
                .Include(a => a.SourceInventory)
                    .ThenInclude(i => i.ReliefStation)
                .Include(a => a.Items)
                    .ThenInclude(i => i.SupplyItem)
                .OrderByDescending(a => a.AllocatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<SupplyAllocation>> GetByInventoryIdAsync(
            Guid inventoryId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(a => a.SourceInventoryId == inventoryId)
                .Include(a => a.Campaign)
                .Include(a => a.Items)
                    .ThenInclude(i => i.SupplyItem)
                .OrderByDescending(a => a.AllocatedAt)
                .ToListAsync(cancellationToken);
        }

        ///// <inheritdoc/>
        public async Task<SupplyAllocation?> GetByIdWithDetailsAsync(
            Guid allocationId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(a => a.Campaign)
                .Include(a => a.SourceInventory)
                    .ThenInclude(i => i.ReliefStation)
                .Include(a => a.Items)
                    .ThenInclude(i => i.SupplyItem)
                .FirstOrDefaultAsync(a => a.AllocationId == allocationId, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<SupplyAllocation>> GetByStatusAsync(
            SupplyAllocationStatus status,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(a => a.Status == status)
                .Include(a => a.Campaign)
                .Include(a => a.SourceInventory)
                    .ThenInclude(i => i.ReliefStation)
                .Include(a => a.Items)
                    .ThenInclude(i => i.SupplyItem)
                .OrderByDescending(a => a.AllocatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
