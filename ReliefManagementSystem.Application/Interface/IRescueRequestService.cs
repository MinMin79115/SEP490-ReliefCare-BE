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

        Task<PaginatedRescueRequestResponseDto> SearchRescueRequestsAsync(
            SearchRescueRequestDto request,
            CancellationToken cancellationToken = default);

        Task<RescueRequestResponseDto> VerifyRescueRequestAsync(
            Guid requestId,
            VerifyRescueRequestDto dto,
            CancellationToken cancellationToken = default);

        Task<RescueRequestResponseDto> AssignTeamToRescueAsync(
            Guid requestId,
            AssignRescueTeamRequestDto dto,
            CancellationToken cancellationToken = default);

        Task<DispatchPreviewResponseDto> PreviewSmartAssignAsync(
            Guid requestId,
            DispatchPreviewRequestDto dto,
            CancellationToken cancellationToken = default);

        Task<RescueBatchQueueResponseDto> SmartAssignTeamToRescueAsync(
            Guid requestId,
            SmartAssignRescueTeamRequestDto dto,
            CancellationToken cancellationToken = default);

        Task<PaginatedDispatchCandidatesResponseDto> GetDispatchCandidatesAsync(
            GetDispatchCandidatesRequestDto dto,
            CancellationToken cancellationToken = default);

        Task<BulkAssignRescueTeamResponseDto> AssignTeamToMultipleRescueRequestsAsync(
            AssignRescueTeamBulkRequestDto dto,
            CancellationToken cancellationToken = default);

        Task<RescueRequestResponseDto> CompleteRescueOperationAsync(
            Guid requestId,
            Guid operationId,
            CompleteRescueOperationRequestDto dto,
            CancellationToken cancellationToken = default);

        Task<RescueRequestResponseDto> UpdateRescueOperationStatusAsync(
            Guid requestId,
            Guid operationId,
            UpdateRescueOperationStatusRequestDto dto,
            CancellationToken cancellationToken = default);

        Task<RescueBatchQueueResponseDto?> GetActiveBatchByTeamAsync(
            Guid teamId,
            CancellationToken cancellationToken = default);

        Task<RescueBatchQueueResponseDto> ReorderBatchQueueAsync(
            Guid teamId,
            ReorderRescueBatchRequestDto dto,
            CancellationToken cancellationToken = default);

        Task RecalculateActiveBatchEtaAsync(
            Guid teamId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Xác minh yêu cầu cứu hộ (Admin/Manager)
        /// - Nếu Approved: tính toán dispatch mode và gửi tới trạm cứu hộ
        /// - Nếu Rejected: cập nhật status thành Cancelled
        /// </summary>
        Task<DistanceMatrixProbeResponse> ProbeDistanceMatrixAsync(
            double originLat,
            double originLng,
            List<double> destinationLats,
            List<double> destinationLngs,
            CancellationToken cancellationToken = default);


        // Extended APIs ────────────────────────────────────────────────────────

        /// <summary>
        /// Lay danh sach yeu cau cuu ho do chinh nguoi dung hien tai gui.
        /// Phan trang va loc theo status.
        /// Dung cho man hinh "Lich su yeu cau cua toi" tren mobile app.
        /// </summary>
        Task<PaginatedRescueRequestResponseDto> GetMyRequestsAsync(
            Guid userId,
            MyRescueRequestQueryDto query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Nguoi dung tu huy yeu cau cuu ho da gui.
        /// Chi cho phep huy khi request dang o trang thai Pending (chua duoc gan team).
        /// Chi chu cua yeu cau moi duoc thuc hien hanh dong nay.
        /// </summary>
        Task<RescueRequestResponseDto> CancelRescueRequestAsync(
            Guid requestId,
            Guid userId,
            CancelRescueRequestDto dto,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lay vi tri realtime (toa do moi nhat) cua doi cuu ho dang xu ly yeu cau.
        /// Khong yeu cau dang nhap (Anonymous) — nguoi dan truy cap qua RequestId.
        /// </summary>
        Task<TeamLocationForRequestDto?> GetTeamLocationForRequestAsync(
            Guid requestId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Thong ke so luong rescue request theo tung trang thai trong he thong.
        /// Dung cho Admin/Moderator dashboard.
        /// </summary>
        Task<RescueRequestStatsDto> GetRescueStatsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Lay lich su cac ca cuu ho (batch) da hoan thanh cua mot team, sap xep moi nhat truoc.
        /// Dung cho man hinh "Lich su ca truc" cua tinh nguyen vien/moderator.
        /// </summary>
        Task<RescueTeamHistoryResponseDto> GetTeamRescueHistoryAsync(
            Guid teamId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}

