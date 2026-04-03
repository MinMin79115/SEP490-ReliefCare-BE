using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        public TeamRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Team?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(t => t.Moderator)
                .Include(t => t.Leader)
                .FirstOrDefaultAsync(t => t.TeamId == id);
        }

        public async Task<Team?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(t => t.Moderator)
                .Include(t => t.Leader)
                .Include(t => t.ReliefStationTeams)
                    .ThenInclude(rst => rst.ReliefStation)
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.User)
                        .ThenInclude(u => u.VolunteerProfile)
                            .ThenInclude(vp => vp.VolunteerSkills)
                                .ThenInclude(vs => vs.Skill)
                .FirstOrDefaultAsync(t => t.TeamId == id);
        }

        public async Task<List<Team>> GetAllAsync()
        {
            return await _dbSet
                .Include(t => t.Moderator)
                .Include(t => t.Leader)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Team>> GetByModeratorIdAsync(Guid moderatorId)
        {
            return await _dbSet
                .Include(t => t.Moderator)
                .Include(t => t.Leader)
                .Where(t => t.CreateBy == moderatorId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public IQueryable<Team> GetQueryable()
        {
            return _dbSet
                .Include(t => t.Moderator)
                .Include(t => t.Leader)
                .AsQueryable();
        }

        public async Task<bool> IsModeratorOfTeamAsync(Guid teamId, Guid userId)
        {
            return await _dbSet
                .AnyAsync(t => t.TeamId == teamId && t.CreateBy == userId);
        }

        public async Task<int> GetTeamMemberCountAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            return await _context.TeamMembers
                .CountAsync(tm => tm.TeamId == teamId, cancellationToken);
        }

        public async Task<int> GetAvailablePeopleCountAsync(CancellationToken cancellationToken = default)
        {
            return await _context.TeamMembers
                .Where(tm => tm.Team.Status == TeamStatus.Active)
                .Where(tm => !_context.Set<CampaignTeam>().Any(ct =>
                    ct.TeamId == tm.TeamId &&
                    !ct.IsDelete &&
                    (ct.Status == CampaignTeamStatus.Accepted || ct.Status == CampaignTeamStatus.Active)))
                .Where(tm => tm.User.VolunteerProfile != null)
                .Where(tm => tm.User.VolunteerProfile!.Status == VolunteerStatus.Active)
                .Where(tm => tm.User.VolunteerProfile!.VerificationStatus == VerificationStatus.Approved)
                .Select(tm => tm.UserId)
                .Distinct()
                .CountAsync(cancellationToken);
        }

        public async Task<int> GetAvailablePeopleCountByTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
        {
            return await _context.TeamMembers
                .Where(tm => tm.TeamId == teamId)
                .Where(tm => tm.Team.Status == TeamStatus.Active)
                .Where(tm => !_context.Set<CampaignTeam>().Any(ct =>
                    ct.TeamId == tm.TeamId &&
                    !ct.IsDelete &&
                    (ct.Status == CampaignTeamStatus.Accepted || ct.Status == CampaignTeamStatus.Active)))
                .Where(tm => tm.User.VolunteerProfile != null)
                .Where(tm => tm.User.VolunteerProfile!.Status == VolunteerStatus.Active)
                .Where(tm => tm.User.VolunteerProfile!.VerificationStatus == VerificationStatus.Approved)
                .Select(tm => tm.UserId)
                .Distinct()
                .CountAsync(cancellationToken);
        }

        public async Task<List<Team>> GetTeamsByModeratorWithMembersAsync(Guid moderatorId)
        {
            return await _dbSet
                .Include(t => t.Moderator)
                .Include(t => t.Leader)
                    .ThenInclude(l => l.VolunteerProfile)
                        .ThenInclude(vp => vp.VolunteerSkills)
                            .ThenInclude(vs => vs.Skill)
                .Include(t => t.TeamMembers)
                    .ThenInclude(tm => tm.User)
                        .ThenInclude(u => u.VolunteerProfile)
                            .ThenInclude(vp => vp.VolunteerSkills)
                                .ThenInclude(vs => vs.Skill)
                .Where(t => t.CreateBy == moderatorId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

    }
}
