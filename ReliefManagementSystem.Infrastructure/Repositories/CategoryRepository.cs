using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
        }

        public async Task<List<Category>> GetAllWithItemsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .Include(c => c.InventoryItems)
                .ToListAsync(cancellationToken);
        }
    }
}
