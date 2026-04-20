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
                .Where(x => x.CampaignTaskId == campaignTaskId)
                .OrderBy(x => x.AssignedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
