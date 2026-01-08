using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class InventoryItemRepository : GenericRepository<InventoryItem>, IInventoryItemRepository
    {
        public InventoryItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<InventoryItem?> GetByIdWithCategoryAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.InventoryItems
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.InventoryItemId == id, cancellationToken);
        }

        public async Task<List<InventoryItem>> GetAllWithCategoryAsync(CancellationToken cancellationToken = default)
        {
            return await _context.InventoryItems
                .Include(i => i.Category)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<InventoryItem>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.InventoryItems
                .Include(i => i.Category)
                .Where(i => i.CategoryId == categoryId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<InventoryItem>> GetByStatusAsync(InventoryStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.InventoryItems
                .Include(i => i.Category)
                .Where(i => i.Status == status)
                .ToListAsync(cancellationToken);
        }

        public async Task<InventoryItem?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.Code == code, cancellationToken);
        }

        public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _context.InventoryItems.Where(i => i.Code == code);
            
            if (excludeId.HasValue)
                query = query.Where(i => i.InventoryItemId != excludeId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<int> CountByStatusAsync(InventoryStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.InventoryItems
                .Where(i => i.Status == status)
                .CountAsync(cancellationToken);
        }

        public async Task<int> CountCreatedTodayAsync(CancellationToken cancellationToken = default)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.InventoryItems
                .Where(i => i.CreatedAt.Date == today)
                .CountAsync(cancellationToken);
        }
    }
}
