using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class RefreshTokenRepository
        : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        private new readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == token);
        }

        public async Task<RefreshToken?> GetActiveByUserIdAsync(Guid userId)
        {
            return await _context.RefreshTokens
                .Where(r =>
                    r.UserId == userId &&
                    r.Revoked == null &&
                    r.Expires > DateTime.UtcNow)
                .OrderByDescending(r => r.Created)
                .FirstOrDefaultAsync();
        }

        public async Task RevokeAllByUserIdAsync(Guid userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(r => r.UserId == userId && r.Revoked == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.Revoked = DateTime.UtcNow;
            }
        }
    }
}
