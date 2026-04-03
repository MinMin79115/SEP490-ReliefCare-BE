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

        Task<RescueRequest?> GetByIdForCompletionAsync(Guid id, CancellationToken cancellationToken = default);

        Task DetachTrackedAttachmentsAsync(Guid requestId, CancellationToken cancellationToken = default);

        Task<List<RescueRequest>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>Lấy tất cả yêu cầu cứu hộ do một người dùng cụ thể gửi, sắp xếp mới nhất trước — dùng cho màn hình lịch sử cá nhân</summary>
        Task<(List<RescueRequest> Items, int TotalCount)> GetByReporterUserIdAsync(
            Guid userId,
            int pageNumber,
            int pageSize,
            int? statusFilter = null,
            CancellationToken cancellationToken = default);

        /// <summary>Đếm số lượng rescue request theo từng trạng thái — dùng cho dashboard stats</summary>
        Task<Dictionary<int, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default);
    }
}
