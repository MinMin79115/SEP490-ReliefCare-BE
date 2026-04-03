using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class ProcurementOrderRepository : GenericRepository<ProcurementOrder>, IProcurementOrderRepository
    {
        public ProcurementOrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ProcurementOrder?> GetWithItemsAsync(Guid procurementOrderId, CancellationToken cancellationToken = default)
        {
            return await _context.ProcurementOrders
                .Include(o => o.Items)
                    .ThenInclude(i => i.SupplyItem)
                .FirstOrDefaultAsync(o => o.ProcurementOrderId == procurementOrderId, cancellationToken);
        }

        public async Task<List<ProcurementOrder>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.ProcurementOrders
                .Include(o => o.Items)
                    .ThenInclude(i => i.SupplyItem)
                .Where(o => o.CampaignId == campaignId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountCreatedOnDateAsync(DateTime dateUtc, CancellationToken cancellationToken = default)
        {
            var nextDate = dateUtc.Date.AddDays(1);
            return await _context.ProcurementOrders
                .CountAsync(o => o.CreatedAt >= dateUtc.Date && o.CreatedAt < nextDate, cancellationToken);
        }
    }
}
