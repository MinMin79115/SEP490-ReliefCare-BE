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
                    .ThenInclude(ro => ro.Team)
                        .ThenInclude(t => t.TrackingPoints)
                .Include(r => r.RescueOperations)
                    .ThenInclude(ro => ro.Team)
                        .ThenInclude(t => t.RescueBatches)
                            .ThenInclude(b => b.Items)
                .Include(r => r.RescueOperations)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                .Include(r => r.RescueOperations)
                    .ThenInclude(ro => ro.ReliefStation)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<RescueRequest?> GetByIdForCompletionAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RescueRequest>()
                .Where(r => r.RequestId == id)
                .Include(r => r.RescueOperations)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<RescueRequest?> GetByIdForCancellationAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RescueRequest>()
                .Where(r => r.RequestId == id)
                .Include(r => r.Verifications)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<RescueRequest?> GetByIdForCancellationUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RescueRequest>()
                .Where(r => r.RequestId == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task DetachTrackedAttachmentsAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            var trackedAttachmentEntries = _context.ChangeTracker.Entries<Attachment>()
                .Where(e => e.Entity.RequestId == requestId)
                .ToList();

            foreach (var entry in trackedAttachmentEntries)
            {
                entry.State = EntityState.Detached;
            }

            await Task.CompletedTask;
        }

        public  async Task<List<RescueRequest>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<RescueRequest>()
                .Include(r => r.Verifications)
                .Include(r => r.Attachments)
                .Include(r => r.RescueRequestPriorities)
                    .ThenInclude(rp => rp.PriorityCriteria)
                .Include(r => r.Campaign)
                .Include(r => r.RescueOperations)
                    .ThenInclude(ro => ro.Team)
                        .ThenInclude(t => t.TrackingPoints)
                .Include(r => r.RescueOperations)
                    .ThenInclude(ro => ro.Team)
                        .ThenInclude(t => t.RescueBatches)
                            .ThenInclude(b => b.Items)
                .Include(r => r.RescueOperations)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                .Include(r => r.RescueOperations)
                    .ThenInclude(ro => ro.ReliefStation)
                .ToListAsync(cancellationToken);
        }
        public async Task<(List<RescueRequest> Items, int TotalCount)> GetByReporterUserIdAsync(
            Guid userId,
            int pageNumber,
            int pageSize,
            int? statusFilter = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<RescueRequest>()
                .Where(r => r.ReporterUserId == userId);

            if (statusFilter.HasValue)
                query = query.Where(r => (int)r.RescueRequestStatus == statusFilter.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(r => r.Attachments)
                .Include(r => r.RescueOperations)
                    .ThenInclude(ro => ro.Team)
                        .ThenInclude(t => t.TrackingPoints)
                .Include(r => r.RescueOperations)
                    .ThenInclude(ro => ro.Team)
                        .ThenInclude(t => t.RescueBatches)
                            .ThenInclude(b => b.Items)
                .Include(r => r.RescueOperations)
                    .ThenInclude(ro => ro.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                .Include(r => r.RescueOperations)
                    .ThenInclude(ro => ro.ReliefStation)
                .Include(r => r.Verifications)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<Dictionary<int, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<RescueRequest>()
                .GroupBy(r => (int)r.RescueRequestStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
        }
    }
}
