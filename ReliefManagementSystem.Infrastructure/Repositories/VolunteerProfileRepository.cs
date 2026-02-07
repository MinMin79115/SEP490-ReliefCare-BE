using Microsoft.AspNetCore.Identity;
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
    public class VolunteerProfileRepository : IVolunteerProfileRepository
    {
        private readonly ApplicationDbContext _context;
        public VolunteerProfileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VolunteerProfile?> GetByUserIdAsync(Guid userId)
        {
            return await _context.VolunteerProfiles
                .Include(vp => vp.User)
                .FirstOrDefaultAsync(vp => vp.UserId == userId);
        }

        public async Task<VolunteerProfile?> GetByUserIdWithSkillsAsync(Guid userId)
        {
            return await _context.VolunteerProfiles
                .Include(vp => vp.User)
                .Include(vp => vp.VolunteerSkills)
                    .ThenInclude(vs => vs.Skill)
                .FirstOrDefaultAsync(vp => vp.UserId == userId);
        }

        public async Task<List<VolunteerProfile>> GetAllWithSkillsAsync()
        {
            return await _context.VolunteerProfiles
                .Include(vp => vp.User)
                .Include(vp => vp.VolunteerSkills)
                    .ThenInclude(vs => vs.Skill)
                .ToListAsync();
        }

        public async Task<VolunteerProfile?> GetByIdWithSkillsAndUserAsync(Guid volunteerProfileId)
        {
            return await _context.VolunteerProfiles
                .Include(vp => vp.User)
                .Include(vp => vp.VolunteerSkills)
                    .ThenInclude(vs => vs.Skill)
                .FirstOrDefaultAsync(vp => vp.VolunteerProfileId == volunteerProfileId);
        }

        public async Task<ApplicationUser?> GetByIdWithVolunteerProfileAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.VolunteerProfile)
                    .ThenInclude(vp => vp.VolunteerSkills)
                        .ThenInclude(vs => vs.Skill)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }


        public async Task AddAsync(VolunteerProfile profile)
        {
            await _context.VolunteerProfiles.AddAsync(profile);
        }

        public Task UpdateAsync(VolunteerProfile profile)
        {
            _context.VolunteerProfiles.Update(profile);
            return Task.CompletedTask;
        }
    }
}
