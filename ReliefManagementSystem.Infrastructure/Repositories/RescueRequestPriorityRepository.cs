using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class RescueRequestPriorityRepository : GenericRepository<RescueRequestPriority>, IRescueRequestPriorityRepository
    {
        public RescueRequestPriorityRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<RescueRequestPriority>> GetByRescueRequestIdAsync(Guid rescueRequestId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RescueRequestPriority>()
                .Where(rp => rp.RescueRequestId == rescueRequestId)
                .Include(rp => rp.PriorityCriteria)
                .ToListAsync(cancellationToken);
        }
    }
}
