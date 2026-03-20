using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class UserRepository
     : GenericRepository<ApplicationUser>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<ApplicationUser?> GetByIdWithVolunteerProfileAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.VolunteerProfile)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        public async Task<ApplicationUser?> GetByIdWithVolunteerProfileAndSkillsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.VolunteerProfile)
                    .ThenInclude(vp => vp.VolunteerSkills)
                        .ThenInclude(vs => vs.Skill)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }

        /// <inheritdoc />
        public IQueryable<ApplicationUser> GetAllUsersQueryable()
        {
            return _context.Users.AsNoTracking().OrderBy(u => u.DisplayName);
        }

        public async Task<ApplicationUser> GetUserById(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            return user;
        }
    }
}
