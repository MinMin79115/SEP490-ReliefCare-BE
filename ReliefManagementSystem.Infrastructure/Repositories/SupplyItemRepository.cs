using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class SupplyItemRepository : GenericRepository<SupplyItem>, ISupplyItemRepository
    {
        public SupplyItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<SupplyItem>> GetByIdsAsync(
            List<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            return await _context.SupplyItems
                .Where(s => ids.Contains(s.SupplyItemId))
                .ToListAsync(cancellationToken);
        }

        public async Task<SupplyItem?> GetByIdWithDetailsAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.SupplyItems
                .Include(s => s.TransactionItems)
                    .ThenInclude(ti => ti.Transaction)
                .FirstOrDefaultAsync(s => s.SupplyItemId == id, cancellationToken);
        }

        public async Task<List<SupplyItem>> GetByCategoryAsync(
            Domain.Enum.SupplyCategory category,
            CancellationToken cancellationToken = default)
        {
            return await _context.SupplyItems
                .Where(s => s.Category == category)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SupplyItem>> GetFilteredAsync(
            Domain.Enum.SupplyCategory? category,
            string? search,
            CancellationToken cancellationToken = default)
        {
            var query = _context.SupplyItems.AsNoTracking();

            // Filter by category at database level
            if (category.HasValue)
            {
                query = query.Where(s => s.Category == category.Value);
            }

            // Filter by search at database level
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(searchLower) ||
                    (s.Description != null && s.Description.ToLower().Contains(searchLower))
                );
            }

            return await query
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
