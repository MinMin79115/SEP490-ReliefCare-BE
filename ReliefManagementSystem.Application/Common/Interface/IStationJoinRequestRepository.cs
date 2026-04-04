using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IStationJoinRequestRepository : IGenericRepository<StationJoinRequest>
    {
        Task<StationJoinRequest?> GetByIdWithDetailsAsync(Guid requestId, CancellationToken cancellationToken = default);
        Task<StationJoinRequest?> GetExistingPendingRequestAsync(Guid teamId, Guid stationId, CancellationToken cancellationToken = default);
        IQueryable<StationJoinRequest> GetQueryableWithDetails();
    }
}
