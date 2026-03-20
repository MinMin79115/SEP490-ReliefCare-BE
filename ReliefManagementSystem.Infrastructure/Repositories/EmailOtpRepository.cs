using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class EmailOtpRepository : GenericRepository<EmailOtp>, IEmailOtpRepository
    {
        public EmailOtpRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<EmailOtp?> GetLatestValidAsync(Guid userId, OtpPurpose purpose, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.Purpose == purpose && x.ConsumedAt == null && x.ExpiresAt > now)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task InvalidateAllActiveAsync(Guid userId, OtpPurpose purpose, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var items = await _dbSet
                .Where(x => x.UserId == userId && x.Purpose == purpose && x.ConsumedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.ConsumedAt = now;
            }
        }
    }
}
