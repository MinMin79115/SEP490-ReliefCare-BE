using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class WarehouseTransactionRepository : GenericRepository<WarehouseTransaction>, IWarehouseTransactionRepository
    {
        public WarehouseTransactionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<WarehouseTransaction>> GetByBatchIdAsync(Guid batchId, CancellationToken cancellationToken = default)
        {
            return await _context.WarehouseTransactions
                .Include(t => t.InventoryItem)
                .Where(t => t.BatchId == batchId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<WarehouseTransaction>> GetByItemIdAsync(Guid itemId, CancellationToken cancellationToken = default)
        {
            return await _context.WarehouseTransactions
                .Include(t => t.Batch)
                .Where(t => t.InventoryItemId == itemId)
                .OrderByDescending(t => t.Batch.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task BulkAddAsync(List<WarehouseTransaction> transactions, CancellationToken cancellationToken = default)
        {
            await _context.WarehouseTransactions.AddRangeAsync(transactions, cancellationToken);
        }
    }
}
