using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class RescueOperationRepository : GenericRepository<RescueOperation>, IRescueOperationRepository
    {
        public RescueOperationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<RescueOperation>> GetByRescueRequestIdAsync(Guid rescueRequestId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RescueOperation>()
                .Where(ro => ro.RescueRequestId == rescueRequestId)
                .Include(ro => ro.ReliefStation)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<RescueOperation>> GetByStationIdAsync(Guid stationId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RescueOperation>()
                .Where(ro => ro.ReliefStationId == stationId)
                .Include(ro => ro.RescueRequest)
                .ToListAsync(cancellationToken);
        }

        public async Task<RescueOperation?> GetActiveByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RescueOperation>()
                .Where(ro => ro.VehicleId == vehicleId)
                .Where(ro => ro.Status != ReliefManagementSystem.Domain.Enum.RescueOperationStatus.RescueCompleted
                    && ro.Status != ReliefManagementSystem.Domain.Enum.RescueOperationStatus.Closed
                    && ro.Status != ReliefManagementSystem.Domain.Enum.RescueOperationStatus.Cancelled)
                .Include(ro => ro.Team)
                .OrderByDescending(ro => ro.StartedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
