using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class MemberTaskDeliveryRepository : GenericRepository<MemberTaskDelivery>, IMemberTaskDeliveryRepository
    {
        public MemberTaskDeliveryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IQueryable<MemberTaskDelivery> GetQueryable()
        {
            return _context.MemberTaskDeliveries
                .Include(x => x.MemberTask)
                .Include(x => x.HouseholdDelivery)
                    .ThenInclude(h => h.CampaignHousehold)
                .Include(x => x.AssignedVolunteerProfile)
                    .ThenInclude(v => v.User)
                .AsQueryable();
        }

        public async Task<List<MemberTaskDelivery>> GetByMemberTaskIdAsync(Guid memberTaskId, CancellationToken cancellationToken = default)
        {
            return await GetQueryable()
                .Where(x => x.MemberTaskId == memberTaskId)
                .OrderBy(x => x.HouseholdDelivery.ScheduledAt)
                .ToListAsync(cancellationToken);
        }
    }
}
