using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class RescueRequestRepository : GenericRepository<RescueRequest>, IRescueRequestRepository
    {
        public RescueRequestRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<RescueRequest>> GetByStatusAsync(int status, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RescueRequest>()
                .Where(r => (int)r.RescueRequestStatus == status)
                .Include(r => r.Attachments)
                .Include(r => r.RescueRequestPriorities)
                .Include(r => r.RescueOperations)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<RescueRequest>> GetByDisasterTypeAsync(int disasterType, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RescueRequest>()
                .Where(r => (int)r.DisasterType == disasterType)
                .Include(r => r.Attachments)
                .Include(r => r.RescueRequestPriorities)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<RescueRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<RescueRequest>()
                .Where(r => r.RescueRequestStatus == RescueRequestStatus.Pending)
                .Include(r => r.Attachments)
                .Include(r => r.RescueRequestPriorities)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<RescueRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RescueRequest>()
                .Where(r => r.RequestId == id)
                .Include(r => r.Verifications)
                .Include(r => r.Attachments)
                .Include(r => r.RescueRequestPriorities)
                    .ThenInclude(rp => rp.PriorityCriteria)
                .Include(r => r.Campaign)
                .Include(r => r.RescueOperations)
                    .ThenInclude(ro => ro.ReliefStation)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public  async Task<List<RescueRequest>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<RescueRequest>()
                .Include(r => r.Attachments)
                .Include(r => r.RescueRequestPriorities)
                .Include(r => r.Campaign)
                .Include(r => r.RescueOperations)
                .ToListAsync(cancellationToken);
        }
    }
}
