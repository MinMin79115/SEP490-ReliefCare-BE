using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    /// <summary>
    /// Concrete implementation of <see cref="ISupplyItemRepository"/>.
    /// </summary>
    public class SupplyItemRepository : GenericRepository<SupplyItem>, ISupplyItemRepository
    {
        public SupplyItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<SupplyItem>> GetAllAsync(
            SupplyCategory? category = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsNoTracking();

            if (category.HasValue)
            {
                query = query.Where(s => s.Category == category.Value);
            }

            return await query
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<SupplyItem?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower(), cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> IsNameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(s => s.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.SupplyItemId != excludeId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }
    }
}
