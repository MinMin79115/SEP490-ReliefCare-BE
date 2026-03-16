using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class StationJoinRequestRepository : GenericRepository<StationJoinRequest>, IStationJoinRequestRepository
    {
        public StationJoinRequestRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<StationJoinRequest?> GetByIdWithDetailsAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.Team)
                .Include(x => x.ReliefStation)
                .Include(x => x.RequestedByLeader)
                .Include(x => x.ReviewedByModerator)
                .FirstOrDefaultAsync(x => x.StationJoinRequestId == requestId, cancellationToken);
        }

        public async Task<StationJoinRequest?> GetExistingPendingRequestAsync(Guid teamId, Guid stationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x =>
                    x.TeamId == teamId &&
                    x.ReliefStationId == stationId &&
                    x.Status == StationJoinRequestStatus.Pending,
                    cancellationToken);
        }

        public IQueryable<StationJoinRequest> GetQueryableWithDetails()
        {
            return _dbSet
                .AsNoTracking()
                .Include(x => x.Team)
                .Include(x => x.ReliefStation)
                .Include(x => x.RequestedByLeader)
                .Include(x => x.ReviewedByModerator)
                .AsQueryable();
        }
    }
}
