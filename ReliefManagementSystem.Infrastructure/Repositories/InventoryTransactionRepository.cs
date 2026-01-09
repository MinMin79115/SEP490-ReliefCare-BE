using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class InventoryTransactionRepository : GenericRepository<InventoryTransaction>, IInventoryTransactionRepository
    {
        public InventoryTransactionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<InventoryTransaction?> GetByIdWithItemsAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.InventoryTransactions
                .Include(t => t.Items)
                    .ThenInclude(ti => ti.SupplyItem)
                .Include(t => t.CreatedByUser)
                .FirstOrDefaultAsync(t => t.TransactionId == id, cancellationToken);
        }

        public async Task<List<InventoryTransaction>> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            return await _context.InventoryTransactions
                .Include(t => t.Items)
                    .ThenInclude(ti => ti.SupplyItem)
                .Include(t => t.CreatedByUser)
                .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<InventoryTransaction>> GetByTypeAsync(
            Domain.Enum.TransactionType type,
            CancellationToken cancellationToken = default)
        {
            return await _context.InventoryTransactions
                .Include(t => t.Items)
                    .ThenInclude(ti => ti.SupplyItem)
                .Include(t => t.CreatedByUser)
                .Where(t => t.Type == type)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
