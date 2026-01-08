using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class BatchRepository : GenericRepository<ImportExportBatch>, IBatchRepository
    {
        public BatchRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ImportExportBatch?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ImportExportBatches
                .Include(b => b.Creator)
                .Include(b => b.Transactions)
                    .ThenInclude(t => t.InventoryItem)
                .FirstOrDefaultAsync(b => b.BatchId == id, cancellationToken);
        }

        public async Task<List<ImportExportBatch>> GetByTypeAsync(
            TransactionType type,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return await _context.ImportExportBatches
                .Include(b => b.Creator)
                .Include(b => b.Transactions)
                .Where(b => b.BatchType == type)
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ImportExportBatch>> GetAllWithDetailsAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            return await _context.ImportExportBatches
                .Include(b => b.Creator)
                .Include(b => b.Transactions)
                .OrderByDescending(b => b.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<string> GenerateBatchNumberAsync(string prefix, CancellationToken cancellationToken = default)
        {
            var year = DateTime.UtcNow.Year;
            var lastBatch = await _context.ImportExportBatches
                .Where(b => b.BatchNumber.StartsWith($"{prefix}-{year}-"))
                .OrderByDescending(b => b.BatchNumber)
                .FirstOrDefaultAsync(cancellationToken);

            int nextNumber = 1;
            if (lastBatch != null)
            {
                var parts = lastBatch.BatchNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}-{year}-{nextNumber:D3}";
        }

        public async Task<decimal> GetTotalExportedTodayAsync(CancellationToken cancellationToken = default)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.WarehouseTransactions
                .Include(t => t.Batch)
                .Where(t => t.Batch.BatchType == TransactionType.Export
                         && t.Batch.CreatedAt.Date == today)
                .SumAsync(t => t.Quantity, cancellationToken);
        }
    }
}
