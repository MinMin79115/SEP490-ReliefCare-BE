using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ITeamTrackingPointRepository : IGenericRepository<TeamTrackingPoint>
    {
        Task<List<TeamTrackingPoint>> GetLatestByTeamAsync(
            Guid teamId,
            int limit = 100,
            CancellationToken cancellationToken = default);

        Task<TeamTrackingPoint?> GetLatestPointAsync(
            Guid teamId,
            CancellationToken cancellationToken = default);
    }
}
