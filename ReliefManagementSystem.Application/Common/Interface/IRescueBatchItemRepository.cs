using ReliefManagementSystem.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IRescueBatchItemRepository : IGenericRepository<RescueBatchItem>
    {
        Task<int> GetMaxSequenceOrderAsync(Guid batchId, CancellationToken ct = default);
    }
}
