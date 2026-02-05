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
    public class TeamMemberRepository : GenericRepository<TeamMember>, ITeamMemberRepository
    {
        public TeamMemberRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<TeamMember?> GetByTeamAndUserAsync(Guid teamId, Guid userId)
        {
            return await _dbSet
                .Include(tm => tm.Team)
                .Include(tm => tm.User)
                .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);
        }

        public async Task<List<TeamMember>> GetByTeamIdWithSkillsAsync(Guid teamId)
        {
            return await _dbSet
                .Include(tm => tm.User)
                    .ThenInclude(u => u.VolunteerProfile)
                        .ThenInclude(vp => vp.VolunteerSkills)
                            .ThenInclude(vs => vs.Skill)
                .Where(tm => tm.TeamId == teamId)
                .OrderBy(tm => tm.RoleTeam)
                .ThenBy(tm => tm.JoinedAt)
                .ToListAsync();
        }

        public async Task<TeamMember?> GetTeamByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(tm => tm.Team)
                    .ThenInclude(t => t.Moderator)
                .Include(tm => tm.Team)
                    .ThenInclude(t => t.Leader)
                .Include(tm => tm.User)
                .FirstOrDefaultAsync(tm => tm.UserId == userId);
        }

        public async Task<List<TeamMember>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(tm => tm.Team)
                .Where(tm => tm.UserId == userId)
                .ToListAsync();
        }
        public async Task<TeamMember?> GetByTeamAndUserWithSkillsAsync(Guid teamId, Guid userId)
        {
            return await _dbSet
                .Include(tm => tm.User)
                    .ThenInclude(u => u.VolunteerProfile)
                        .ThenInclude(vp => vp.VolunteerSkills)
                            .ThenInclude(vs => vs.Skill)
                .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);
        }
        public async Task<bool> IsMemberAsync(Guid teamId, Guid userId)
        {
            return await _dbSet
                .AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId);
        }

        public async Task<TeamMember?> GetMemberAsync(Guid teamId, Guid userId)
        {
            return await GetByTeamAndUserAsync(teamId, userId);
        }

        public IQueryable<TeamMember> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task AddAsync(TeamMember teamMember)
        {
            await _dbSet.AddAsync(teamMember);
        }

        public Task UpdateAsync(TeamMember teamMember)
        {
            _dbSet.Update(teamMember);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(TeamMember teamMember)
        {
            _dbSet.Remove(teamMember);
            return Task.CompletedTask;
        }
    }
}
