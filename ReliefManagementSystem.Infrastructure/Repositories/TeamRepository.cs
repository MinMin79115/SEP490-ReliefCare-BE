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
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _context;

        public TeamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Team?> GetByIdAsync(Guid id)
        {
            return await _context.Teams
                .Include(t => t.Moderator)
                .Include(t => t.Leader)
                .FirstOrDefaultAsync(t => t.TeamId == id);
        }

        public async Task<Team?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Teams
                .Include(t => t.Moderator)
                .Include(t => t.Leader)
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.User)
                        .ThenInclude(u => u.VolunteerProfile)
                            .ThenInclude(vp => vp.VolunteerSkills)
                                .ThenInclude(vs => vs.Skill)
                .FirstOrDefaultAsync(t => t.TeamId == id);
        }

        public async Task<List<Team>> GetAllAsync()
        {
            return await _context.Teams
                .Include(t => t.Moderator)
                .Include(t => t.Leader)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Team>> GetByModeratorIdAsync(Guid moderatorId)
        {
            return await _context.Teams
                .Include(t => t.Moderator)
                .Include(t => t.Leader)
                .Where(t => t.ModeratorId == moderatorId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public IQueryable<Team> GetQueryable()
        {
            return _context.Teams
                .Include(t => t.Moderator)
                .Include(t => t.Leader)
                .AsQueryable();
        }

        public async Task AddAsync(Team team)
        {
            await _context.Teams.AddAsync(team);
        }

        public Task UpdateAsync(Team team)
        {
            _context.Teams.Update(team);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Team team)
        {
            _context.Teams.Remove(team);
            return Task.CompletedTask;
        }

        public async Task<bool> IsModeratorOfTeamAsync(Guid teamId, Guid userId)
        {
            return await _context.Teams
                .AnyAsync(t => t.TeamId == teamId && t.ModeratorId == userId);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Teams.AnyAsync(t => t.TeamId == id);
        }

        public async Task<int> GetTeamMemberCountAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            return await _context.TeamMembers
                .CountAsync(tm => tm.TeamId == teamId, cancellationToken);
        }
    }
}
