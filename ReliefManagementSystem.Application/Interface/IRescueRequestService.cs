using ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request;
using ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Interface
{
        public interface IRescueRequestService
    {
        /// <summary>
        /// Gửi yêu cầu cứu hộ mới
        /// - Tự động tính priority dựa trên attachments và tiêu chí
        /// - Nếu loại Emergency thì bypass xác minh và dispatch ngay
        /// - Nếu loại Normal thì chờ xác minh trước khi dispatch
        /// </summary>
        Task<RescueRequestResponseDto> CreateRescueRequestAsync(
            CreateRescueRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>Lấy chi tiết yêu cầu cứu hộ theo ID</summary>
        Task<RescueRequestResponseDto> GetRescueRequestByIdAsync(
            Guid requestId,
            CancellationToken cancellationToken = default);

        /// <summary>Lấy danh sách yêu cầu cứu hộ (với phân trang)</summary>
        Task<PaginatedRescueRequestResponseDto> GetRescueRequestsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            int? statusFilter = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Xác minh yêu cầu cứu hộ (Admin/Manager)
        /// - Nếu Approved: tính toán dispatch mode và gửi tới trạm cứu hộ
        /// - Nếu Rejected: cập nhật status thành Cancelled
        /// </summary>
        Task<RescueRequestResponseDto> VerifyRescueRequestAsync(
            Guid requestId,
            VerifyRescueRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tính toán priority score dựa trên các tiêu chí
        /// - Số lượng attachments, loại file, mức độ thảm họa, vv.
        /// </summary>
        //Task<int> CalculatePriorityAsync(
        //    Guid requestId,
        //    CancellationToken cancellationToken = default);

        /// <summary>
        /// Dispatch yêu cầu cứu hộ tới các trạm cứu trợ dựa trên priority level
        /// - Low: 1 trạm gần nhất
        /// - Medium: 2 trạm gần nhất
        /// - High/Critical: tất cả trạm trong vùng
        /// </summary>
        Task DispatchToStationsAsync(
            Guid requestId,
            CancellationToken cancellationToken = default);

        /// <summary>Cập nhật trạng thái yêu cầu cứu hộ</summary>
        Task UpdateRescueRequestStatusAsync(
            Guid requestId,
            int newStatus,
            CancellationToken cancellationToken = default);
    }
}
