using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class MemberTaskRepository : GenericRepository<MemberTask>, IMemberTaskRepository
    {
        public MemberTaskRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<MemberTask>> GetByCampaignTaskIdAsync(Guid campaignTaskId, CancellationToken cancellationToken = default)
        {
            return await _context.MemberTasks
                .Include(x => x.VolunteerProfile)
                .Include(x => x.MemberTaskDeliveries)
                    .ThenInclude(d => d.HouseholdDelivery)
                        .ThenInclude(h => h.CampaignHousehold)
                .Where(x => x.CampaignTaskId == campaignTaskId)
                .OrderBy(x => x.AssignedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<MemberTask?> GetByIdWithDetailsAsync(Guid memberTaskId, CancellationToken cancellationToken = default)
        {
            return await _context.MemberTasks
                .Include(x => x.VolunteerProfile)
                    .ThenInclude(v => v.User)
                .Include(x => x.CampaignTask)
                    .ThenInclude(t => t.CampaignTeam)
                        .ThenInclude(ct => ct.Team)
                .Include(x => x.MemberTaskDeliveries)
                    .ThenInclude(d => d.HouseholdDelivery)
                        .ThenInclude(h => h.CampaignHousehold)
                .Include(x => x.MemberTaskDeliveries)
                    .ThenInclude(d => d.AssignedVolunteerProfile)
                        .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(x => x.MemberTaskId == memberTaskId, cancellationToken);
        }

        public IQueryable<MemberTask> GetQueryable()
        {
            return _context.MemberTasks
                .Include(x => x.VolunteerProfile)
                    .ThenInclude(v => v.User)
                .Include(x => x.CampaignTask)
                    .ThenInclude(t => t.CampaignTeam)
                        .ThenInclude(ct => ct.Team)
                .Include(x => x.MemberTaskDeliveries)
                    .ThenInclude(d => d.HouseholdDelivery)
                        .ThenInclude(h => h.CampaignHousehold);
        }
    }
}
