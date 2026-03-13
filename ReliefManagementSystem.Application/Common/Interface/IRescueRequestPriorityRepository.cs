using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IRescueRequestPriorityRepository : IGenericRepository<RescueRequestPriority>
    {
        Task<List<RescueRequestPriority>> GetByRescueRequestIdAsync(Guid rescueRequestId, CancellationToken cancellationToken = default);
    }
}
