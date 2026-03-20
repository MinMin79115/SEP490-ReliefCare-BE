using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    /// <summary>EF Core implementation of IManagerProfileRepository.</summary>
    public class ManagerProfileRepository : GenericRepository<ManagerProfile>, IManagerProfileRepository
    {
        public ManagerProfileRepository(ApplicationDbContext context) : base(context) { }

        /// <inheritdoc/>
        public async Task<ManagerProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.ManagerProfiles
                .Include(mp => mp.AssignedLocation)
                .FirstOrDefaultAsync(mp => mp.UserId == userId, ct);
        }
    }
}
