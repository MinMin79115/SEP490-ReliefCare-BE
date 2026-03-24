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

    }
}
