using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IProcurementOrderRepository : IGenericRepository<ProcurementOrder>
    {
        Task<ProcurementOrder?> GetWithItemsAsync(Guid procurementOrderId, CancellationToken cancellationToken = default);
        Task<List<ProcurementOrder>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<int> CountCreatedOnDateAsync(DateTime dateUtc, CancellationToken cancellationToken = default);
    }
}
