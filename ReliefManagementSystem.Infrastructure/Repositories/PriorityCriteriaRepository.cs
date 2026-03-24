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
    public class PriorityCriteriaRepository : GenericRepository<PriorityCriteria>, IPriorityCriteriaRepository
    {
        public PriorityCriteriaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<PriorityCriteria>> GetByDisasterTypeAsync(DisasterType disasterType, CancellationToken cancellationToken = default)
        {
            return await _context.Set<PriorityCriteria>()
                .Where(p => p.DisasterType == disasterType)
                .Include(p => p.RescueRequestPriorities)
                .ToListAsync(cancellationToken);
        }

        public async Task<PriorityCriteria?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _context.Set<PriorityCriteria>()
                .Where(p => p.Code == code)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public IQueryable<PriorityCriteria> GetQueryable()
        {
            return _context.Set<PriorityCriteria>().AsNoTracking().AsQueryable();
        }
    }
}
