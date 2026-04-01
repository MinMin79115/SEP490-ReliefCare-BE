using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IDistributionSessionRepository : IGenericRepository<DistributionSession>
    {
        Task<DistributionSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<(List<DistributionSession> Items, int TotalCount)> SearchAsync(
            string? search,
            int pageNumber,
            int pageSize,
            DistributionSessionStatus? status,
            Guid? campaignId,
            Guid? reliefStationId,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsRequestAssignmentAsync(Guid distributionSessionId, Guid reliefRequestId, CancellationToken cancellationToken = default);
    }
}
