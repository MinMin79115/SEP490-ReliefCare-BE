using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IPriorityCriteriaRepository : IGenericRepository<PriorityCriteria>
    {
        Task<List<PriorityCriteria>> GetByDisasterTypeAsync(DisasterType disasterType,
            CancellationToken cancellationToken = default);
        Task<PriorityCriteria?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        IQueryable<PriorityCriteria> GetQueryable();
    }
}
