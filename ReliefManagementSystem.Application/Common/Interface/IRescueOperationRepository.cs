using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IRescueOperationRepository : IGenericRepository<RescueOperation>
    {
        Task<List<RescueOperation>> GetByRescueRequestIdAsync(Guid rescueRequestId,
            CancellationToken cancellationToken = default);

        Task<List<RescueOperation>> GetByStationIdAsync(Guid stationId, CancellationToken cancellationToken = default);
    }
}
