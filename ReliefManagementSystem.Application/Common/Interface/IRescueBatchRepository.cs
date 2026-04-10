using ReliefManagementSystem.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IRescueBatchRepository : IGenericRepository<RescueBatch>
    {
        Task<RescueBatch?> GetActiveByTeamIdAsync(Guid teamId, CancellationToken ct = default);

        Task<List<RescueBatch>> GetAllActiveWithItemsAsync(CancellationToken ct = default);

        Task<RescueBatch?> GetByIdWithItemsAsync(Guid batchId, CancellationToken ct = default);

        /// <summary>Lấy danh sách batch đã hoàn thành (Completed) của team theo phân trang — dùng cho màn hình lịch sử</summary>
        Task<(List<RescueBatch> Items, int TotalCount)> GetCompletedByTeamIdAsync(
            Guid teamId,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);
    }
}
