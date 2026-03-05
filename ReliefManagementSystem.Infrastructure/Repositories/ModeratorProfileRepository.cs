using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    /// <summary>EF Core implementation of IModeratorProfileRepository.</summary>
    public class ModeratorProfileRepository : GenericRepository<ModeratorProfile>, IModeratorProfileRepository
    {
        public ModeratorProfileRepository(ApplicationDbContext context) : base(context) { }

        /// <inheritdoc/>
        public async Task<ModeratorProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _context.ModeratorProfiles
                .Include(mp => mp.ReliefStation)
                .FirstOrDefaultAsync(mp => mp.UserId == userId, ct);
        }

        /// <inheritdoc/>
        public async Task<ModeratorProfile?> GetStationHeadAsync(Guid stationId, CancellationToken ct = default)
        {
            return await _context.ModeratorProfiles
                .FirstOrDefaultAsync(mp => mp.ReliefStationId == stationId && mp.IsStationHead, ct);
        }
    }
}
