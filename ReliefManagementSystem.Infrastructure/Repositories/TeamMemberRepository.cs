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
    public class TeamMemberRepository : ITeamMemberRepository
    {
        private readonly ApplicationDbContext _context;

        public TeamMemberRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TeamMember?> GetByTeamAndUserAsync(Guid teamId, Guid userId)
        {
            return await _context.TeamMembers
                .Include(tm => tm.Team)
                .Include(tm => tm.User)
                .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);
        }

        public async Task<List<TeamMember>> GetByTeamIdWithSkillsAsync(Guid teamId)
        {
            return await _context.TeamMembers
                .Include(tm => tm.User)
                    .ThenInclude(u => u.VolunteerProfile)
                        .ThenInclude(vp => vp.VolunteerSkills)
                            .ThenInclude(vs => vs.Skill)
                .Where(tm => tm.TeamId == teamId)
                .OrderBy(tm => tm.RoleTeam)
                .ThenBy(tm => tm.JoinedAt)
                .ToListAsync();
        }

        public async Task<List<TeamMember>> GetByUserIdAsync(Guid userId)
        {
            return await _context.TeamMembers
                .Include(tm => tm.Team)
                .Where(tm => tm.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> IsMemberAsync(Guid teamId, Guid userId)
        {
            return await _context.TeamMembers
                .AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId);
        }

        public async Task AddAsync(TeamMember teamMember)
        {
            await _context.TeamMembers.AddAsync(teamMember);
        }

        public Task UpdateAsync(TeamMember teamMember)
        {
            _context.TeamMembers.Update(teamMember);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(TeamMember teamMember)
        {
            _context.TeamMembers.Remove(teamMember);
            return Task.CompletedTask;
        }
    }
}
