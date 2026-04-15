using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class HouseholdDeliveryProofRepository : GenericRepository<HouseholdDeliveryProof>, IHouseholdDeliveryProofRepository
    {
        public HouseholdDeliveryProofRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<HouseholdDeliveryProof>> GetByDeliveryAsync(Guid householdDeliveryId, CancellationToken cancellationToken = default)
        {
            return await _context.HouseholdDeliveryProofs
                .Where(x => x.HouseholdDeliveryId == householdDeliveryId)
                .OrderByDescending(x => x.CapturedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
