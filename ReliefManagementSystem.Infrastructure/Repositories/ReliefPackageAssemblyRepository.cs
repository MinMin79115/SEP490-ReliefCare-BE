using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class ReliefPackageAssemblyRepository : GenericRepository<ReliefPackageAssembly>, IReliefPackageAssemblyRepository
    {
        public ReliefPackageAssemblyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ReliefPackageAssembly?> GetByIdWithDetailsAsync(Guid reliefPackageAssemblyId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ReliefPackageAssembly>()
                .Include(x => x.Campaign)
                .Include(x => x.ReliefStation)
                .Include(x => x.Inventory)
                .Include(x => x.ReliefPackageDefinition)
                .Include(x => x.OutputSupplyItem)
                .Include(x => x.CreatedByUser)
                .Include(x => x.Details)
                    .ThenInclude(d => d.SupplyItem)
                .FirstOrDefaultAsync(x => x.ReliefPackageAssemblyId == reliefPackageAssemblyId, cancellationToken);
        }

        public async Task<IReadOnlyList<ReliefPackageAssembly>> GetByInventoryAsync(Guid inventoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ReliefPackageAssembly>()
                .AsNoTracking()
                .Include(x => x.ReliefPackageDefinition)
                .Include(x => x.OutputSupplyItem)
                .Include(x => x.CreatedByUser)
                .Include(x => x.Details)
                    .ThenInclude(d => d.SupplyItem)
                .Where(x => x.InventoryId == inventoryId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ReliefPackageAssembly>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ReliefPackageAssembly>()
                .AsNoTracking()
                .Include(x => x.ReliefPackageDefinition)
                .Include(x => x.OutputSupplyItem)
                .Include(x => x.CreatedByUser)
                .Include(x => x.Details)
                    .ThenInclude(d => d.SupplyItem)
                .Where(x => x.CampaignId == campaignId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ReliefPackageAssembly>> GetByStationAsync(Guid campaignId, Guid reliefStationId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ReliefPackageAssembly>()
                .AsNoTracking()
                .Include(x => x.ReliefPackageDefinition)
                .Include(x => x.OutputSupplyItem)
                .Include(x => x.CreatedByUser)
                .Include(x => x.Details)
                    .ThenInclude(d => d.SupplyItem)
                .Where(x => x.CampaignId == campaignId && x.ReliefStationId == reliefStationId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ReliefPackageAssembly>> GetByPackageDefinitionAsync(Guid campaignId, Guid reliefPackageDefinitionId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ReliefPackageAssembly>()
                .AsNoTracking()
                .Include(x => x.ReliefPackageDefinition)
                .Include(x => x.OutputSupplyItem)
                .Include(x => x.CreatedByUser)
                .Include(x => x.Details)
                    .ThenInclude(d => d.SupplyItem)
                .Where(x => x.CampaignId == campaignId && x.ReliefPackageDefinitionId == reliefPackageDefinitionId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
