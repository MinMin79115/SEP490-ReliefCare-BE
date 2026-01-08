using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    /// <summary>
    /// Repository for ImportExportBatch entity
    /// </summary>
    public interface IBatchRepository : IGenericRepository<ImportExportBatch>
    {
        Task<ImportExportBatch?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ImportExportBatch>> GetByTypeAsync(
            TransactionType type, 
            int page, 
            int pageSize, 
            CancellationToken cancellationToken = default);
        Task<List<ImportExportBatch>> GetAllWithDetailsAsync(
            int page, 
            int pageSize, 
            CancellationToken cancellationToken = default);
        Task<string> GenerateBatchNumberAsync(string prefix, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalExportedTodayAsync(CancellationToken cancellationToken = default);
    }
}
