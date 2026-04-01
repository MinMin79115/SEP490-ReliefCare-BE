using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IReliefRequestRepository : IGenericRepository<ReliefRequest>
    {
        Task<ReliefRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<(List<ReliefRequest> Items, int TotalCount)> SearchAsync(
            string? search,
            int pageNumber,
            int pageSize,
            ReliefManagementSystem.Domain.Enum.ReliefRequestStatus? status,
            Guid? assignedStationId,
            Guid? campaignId,
            CancellationToken cancellationToken = default);

        Task<List<ReliefRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
    }
}
