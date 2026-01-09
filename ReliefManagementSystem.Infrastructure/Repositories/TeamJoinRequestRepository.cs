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
    public class TeamJoinRequestRepository : ITeamJoinRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public TeamJoinRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TeamJoinRequest?> GetByIdAsync(Guid id)
        {
            return await _context.TeamJoinRequests
                .Include(tjr => tjr.Team)
                .Include(tjr => tjr.Volunteer)
                .FirstOrDefaultAsync(tjr => tjr.Id == id);
        }

        public async Task<TeamJoinRequest?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.TeamJoinRequests
                .Include(tjr => tjr.Team)
                    .ThenInclude(t => t.Moderator)
                .Include(tjr => tjr.Volunteer)
                    .ThenInclude(v => v.VolunteerProfile)
                        .ThenInclude(vp => vp.VolunteerSkills)
                            .ThenInclude(vs => vs.Skill)
                .Include(tjr => tjr.Reviewer)
                .FirstOrDefaultAsync(tjr => tjr.Id == id);
        }

        public async Task<List<TeamJoinRequest>> GetByVolunteerIdWithDetailsAsync(Guid volunteerId)
        {
            return await _context.TeamJoinRequests
                .Include(tjr => tjr.Team)
                    .ThenInclude(t => t.Moderator)
                .Include(tjr => tjr.Volunteer)
                    .ThenInclude(v => v.VolunteerProfile)
                        .ThenInclude(vp => vp.VolunteerSkills)
                            .ThenInclude(vs => vs.Skill)
                .Include(tjr => tjr.Reviewer)
                .Where(tjr => tjr.VolunteerId == volunteerId)
                .OrderByDescending(tjr => tjr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TeamJoinRequest>> GetByTeamIdWithDetailsAsync(Guid teamId)
        {
            return await _context.TeamJoinRequests
                .Include(tjr => tjr.Team)
                    .ThenInclude(t => t.Moderator)
                .Include(tjr => tjr.Volunteer)
                    .ThenInclude(v => v.VolunteerProfile)
                        .ThenInclude(vp => vp.VolunteerSkills)
                            .ThenInclude(vs => vs.Skill)
                .Include(tjr => tjr.Reviewer)
                .Where(tjr => tjr.TeamId == teamId)
                .OrderByDescending(tjr => tjr.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<TeamJoinRequest>> GetPendingRequestsByModeratorWithDetailsAsync(Guid moderatorId)
        {
            return await _context.TeamJoinRequests
                .Include(tjr => tjr.Team)
                    .ThenInclude(t => t.Moderator)
                .Include(tjr => tjr.Volunteer)
                    .ThenInclude(v => v.VolunteerProfile)
                        .ThenInclude(vp => vp.VolunteerSkills)
                            .ThenInclude(vs => vs.Skill)
                .Where(tjr => tjr.Team.ModeratorId == moderatorId &&
                             tjr.Status == TeamJoinRequestStatus.Pending)
                .OrderBy(tjr => tjr.CreatedAt)
                .ToListAsync();
        }

        public async Task<TeamJoinRequest?> GetExistingPendingRequestAsync(Guid teamId, Guid volunteerId)
        {
            return await _context.TeamJoinRequests
                .FirstOrDefaultAsync(tjr =>
                    tjr.TeamId == teamId &&
                    tjr.VolunteerId == volunteerId &&
                    tjr.Status == TeamJoinRequestStatus.Pending);
        }

        public async Task AddAsync(TeamJoinRequest request)
        {
            await _context.TeamJoinRequests.AddAsync(request);
        }

        public Task UpdateAsync(TeamJoinRequest request)
        {
            _context.TeamJoinRequests.Update(request);
            return Task.CompletedTask;
        }
    }
}
