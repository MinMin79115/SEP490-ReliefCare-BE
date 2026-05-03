using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    /// <summary>
    /// Basic Campaign repository — provides ExistsAsync for validation.
    /// Full Campaign CRUD will be added in the Campaign module.
    /// </summary>
    public class CampaignRepository : GenericRepository<Campaign>, ICampaignRepository
    {
        public CampaignRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Campaign?> GetWithGoalsAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.Campaigns
                .Include(c => c.ResourceGoals)
                .FirstOrDefaultAsync(c => c.CampaignId == campaignId, cancellationToken);
        }

        public async Task<Campaign?> GetWithStationsAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.Campaigns
                .Include(c => c.CampaignStations)
                    .ThenInclude(cs => cs.ReliefStation)
                .FirstOrDefaultAsync(c => c.CampaignId == campaignId, cancellationToken);
        }

        public async Task<Campaign?> GetWithDetailsAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.Campaigns
                .Include(c => c.ResourceGoals)
                .Include(c => c.CampaignStations)
                    .ThenInclude(cs => cs.ReliefStation)
                .Include(c => c.CampaignTeams)
                    .ThenInclude(ct => ct.Team)
                .FirstOrDefaultAsync(c => c.CampaignId == campaignId, cancellationToken);
        }

        public async Task<(List<Campaign> Items, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword,
            CampaignStatus? status,
            CampaignType? type,
            Guid? locationId,
            Guid? reliefStationId,
            bool forVolunteerRegistration,
            CancellationToken cancellationToken = default)
        {
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _context.Campaigns
                .Include(c => c.ResourceGoals)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c => c.Name.Contains(keyword));
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            if (type.HasValue)
            {
                query = query.Where(c => c.Type == type.Value);
            }

            if (locationId.HasValue)
            {
                query = query.Where(c => c.LocationId == locationId.Value);
            }

            if (reliefStationId.HasValue)
            {
                query = query.Where(c => c.CampaignStations.Any(cs => cs.ReliefStationId == reliefStationId.Value && cs.IsActive));
            }

            if (forVolunteerRegistration)
            {
                query = query.Where(c =>
                    c.Type == CampaignType.Fundraising &&
                    c.Status == CampaignStatus.Active &&
                    c.ResourceGoals.Any(g => g.ResourceType == CampaignResourceType.People));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<bool> HasAnyActiveStationAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.CampaignStations
                .AnyAsync(cs => cs.CampaignId == campaignId && cs.IsActive, cancellationToken);
        }

        public async Task<bool> IsStationAlreadyAttachedAsync(Guid campaignId, Guid reliefStationId, CancellationToken cancellationToken = default)
        {
            return await _context.CampaignStations
                .AnyAsync(cs => cs.CampaignId == campaignId && cs.ReliefStationId == reliefStationId, cancellationToken);
        }

        public async Task AddStationAsync(CampaignStation campaignStation, CancellationToken cancellationToken = default)
        {
            await _context.CampaignStations.AddAsync(campaignStation, cancellationToken);
        }

        public async Task<CampaignStation?> GetStationAsync(Guid campaignId, Guid reliefStationId, CancellationToken cancellationToken = default)
        {
            return await _context.CampaignStations
                .Include(cs => cs.ReliefStation)
                .FirstOrDefaultAsync(cs => cs.CampaignId == campaignId && cs.ReliefStationId == reliefStationId, cancellationToken);
        }

        public async Task<CampaignResourceGoal?> GetGoalAsync(Guid campaignId, CampaignResourceType resourceType, CancellationToken cancellationToken = default)
        {
            return await _context.CampaignResourceGoals
                .FirstOrDefaultAsync(g => g.CampaignId == campaignId && g.ResourceType == resourceType, cancellationToken);
        }

        public async Task<List<CampaignResourceGoal>> GetGoalsAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.CampaignResourceGoals
                .Where(g => g.CampaignId == campaignId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddGoalAsync(CampaignResourceGoal goal, CancellationToken cancellationToken = default)
        {
            await _context.CampaignResourceGoals.AddAsync(goal, cancellationToken);
        }

        public Task UpdateGoalAsync(CampaignResourceGoal goal, CancellationToken cancellationToken = default)
        {
            _context.CampaignResourceGoals.Update(goal);
            return Task.CompletedTask;
        }

        public async Task<CampaignTeam?> GetCampaignTeamAsync(Guid campaignId, Guid teamId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<CampaignTeam>()
                .Include(ct => ct.Team)
                .FirstOrDefaultAsync(ct => ct.CampaignId == campaignId && ct.TeamId == teamId && !ct.IsDelete, cancellationToken);
        }

        public async Task<List<CampaignTeam>> GetCampaignTeamsAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<CampaignTeam>()
                .Include(ct => ct.Team)
                .Where(ct => ct.CampaignId == campaignId && !ct.IsDelete)
                .OrderBy(ct => ct.AssignedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddCampaignTeamAsync(CampaignTeam campaignTeam, CancellationToken cancellationToken = default)
        {
            await _context.Set<CampaignTeam>().AddAsync(campaignTeam, cancellationToken);
        }

        public Task UpdateCampaignTeamAsync(CampaignTeam campaignTeam, CancellationToken cancellationToken = default)
        {
            _context.Set<CampaignTeam>().Update(campaignTeam);
            return Task.CompletedTask;
        }

        public async Task<List<Campaign>> GetActiveReliefCampaignsByStationAsync(Guid reliefStationId, CancellationToken cancellationToken = default)
        {
            return await _context.Campaigns
                .Include(c => c.CampaignStations)
                .Where(c => c.Type == CampaignType.Relief
                    && c.Status == CampaignStatus.Active
                    && c.CampaignStations.Any(cs => cs.ReliefStationId == reliefStationId && cs.IsActive))
                .ToListAsync(cancellationToken);
        }

        public IQueryable<Campaign> GetQueryable()
        {
            return _context.Campaigns
                .Include(c => c.CampaignTeams)
                    .ThenInclude(ct => ct.Team)
                        .ThenInclude(t => t.ReliefStationTeams);
        }
    }
}
