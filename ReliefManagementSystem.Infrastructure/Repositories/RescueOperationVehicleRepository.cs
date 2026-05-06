using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class RescueOperationVehicleRepository : GenericRepository<RescueOperationVehicle>, IRescueOperationVehicleRepository
    {
        public RescueOperationVehicleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<RescueOperationVehicle>> GetByOperationIdAsync(Guid rescueOperationId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RescueOperationVehicle>()
                .Where(x => x.RescueOperationId == rescueOperationId)
                .ToListAsync(cancellationToken);
        }

        public async Task ReplaceForOperationAsync(Guid rescueOperationId, List<RescueOperationVehicle> vehicles, CancellationToken cancellationToken = default)
        {
            var existing = await _context.Set<RescueOperationVehicle>()
                .Where(x => x.RescueOperationId == rescueOperationId)
                .ToListAsync(cancellationToken);

            if (existing.Count > 0)
            {
                _context.Set<RescueOperationVehicle>().RemoveRange(existing);
            }

            if (vehicles.Count > 0)
            {
                await _context.Set<RescueOperationVehicle>().AddRangeAsync(vehicles, cancellationToken);
            }
        }
    }
}
