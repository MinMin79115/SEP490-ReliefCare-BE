using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IRescueRequestRepository : IGenericRepository<RescueRequest>
    {
        Task<List<RescueRequest>> GetByStatusAsync(int status, CancellationToken cancellationToken = default);

        Task<List<RescueRequest>> GetByDisasterTypeAsync(int disasterType,
            CancellationToken cancellationToken = default);

        Task<List<RescueRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);

        Task<RescueRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<List<RescueRequest>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
