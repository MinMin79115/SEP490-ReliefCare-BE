using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class CampaignTaskRepository : GenericRepository<CampaignTask>, ICampaignTaskRepository
    {
        public CampaignTaskRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<CampaignTask?> GetByIdWithDetailsAsync(Guid campaignTaskId, CancellationToken cancellationToken = default)
        {
            return await _context.CampaignTasks
                .Include(x => x.CampaignTeam)
                    .ThenInclude(x => x.Team)
                .Include(x => x.MemberTasks)
                    .ThenInclude(x => x.VolunteerProfile)
                .FirstOrDefaultAsync(x => x.CampaignTaskId == campaignTaskId, cancellationToken);
        }

        public async Task<(List<CampaignTask> Items, int TotalCount)> GetPagedByCampaignAsync(
            Guid campaignId,
            int pageIndex,
            int pageSize,
            CampaignTaskStatus? status,
            Guid? campaignTeamId,
            CancellationToken cancellationToken = default)
        {
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _context.CampaignTasks
                .Include(x => x.CampaignTeam)
                    .ThenInclude(x => x.Team)
                .Where(x => x.CampaignTeam.CampaignId == campaignId && !x.CampaignTeam.IsDelete)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (campaignTeamId.HasValue)
            {
                query = query.Where(x => x.CampaignTeamId == campaignTeamId.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public IQueryable<CampaignTask> GetQueryable()
        {
            return _context.CampaignTasks
                .Include(x => x.CampaignTeam)
                    .ThenInclude(x => x.Team)
                .Include(x => x.MemberTasks);
        }
    }
}
