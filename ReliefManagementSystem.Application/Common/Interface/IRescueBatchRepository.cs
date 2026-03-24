using ReliefManagementSystem.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IRescueBatchRepository : IGenericRepository<RescueBatch>
    {
        Task<RescueBatch?> GetActiveByTeamIdAsync(Guid teamId, CancellationToken ct = default);

        Task<RescueBatch?> GetByIdWithItemsAsync(Guid batchId, CancellationToken ct = default);
    }
}
