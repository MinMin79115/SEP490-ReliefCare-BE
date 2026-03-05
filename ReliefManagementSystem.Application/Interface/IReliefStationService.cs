using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Request;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Response;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Interface
{
    /// <summary>Service contract for ReliefStation and team assignment operations.</summary>
    public interface IReliefStationService
    {
        /// <summary>
        /// Lấy danh sách tất cả trạm cứu trợ có phân trang.
        /// Hỗ trợ lọc theo Level (Regional/Provincial/Local) và tìm kiếm theo tên.
        /// </summary>
        Task<Pagination<ReliefStationResponse>> GetAllStationsAsync(
            GetAllStationsRequest request,
            CancellationToken ct = default);

        /// <summary>
        /// Tạo trạm cứu trợ cấp Tỉnh (Provincial).
        /// Chỉ Manager mới được gọi API này.
        /// Hệ thống tự động gán <c>ParentReliefStationId</c> = trạm Regional
        /// của vùng mà Manager phụ trách, dựa theo <c>LocationId</c> truyền vào.
        /// </summary>
        Task<ReliefStationResponse> CreateProvincialStationAsync(
            CreateProvincialStationRequest request,
            CancellationToken ct = default);

        /// <summary>
        /// Tạo trạm cứu trợ cấp Địa phương (Local).
        /// Chỉ Moderator (IsStationHead = true) tại trạm tỉnh mới được gọi API này.
        /// Hệ thống tự động gán <c>ParentReliefStationId</c> = trạm Provincial
        /// mà Moderator đang đứng đầu.
        /// </summary>
        Task<ReliefStationResponse> CreateLocalStationAsync(
            CreateLocalStationRequest request,
            CancellationToken ct = default);

        /// <summary>
        /// Gán Moderator vào một trạm cứu trợ.
        /// - Admin có thể gán cho bất kỳ trạm nào.
        /// - Manager chỉ có thể gán cho các trạm nằm trong vùng mình phân công (Regional/Provincial/Local).
        /// - Moderator trưởng trạm tỉnh có thể gán cho trạm Local (nơi họ là Parent).
        /// </summary>
        Task<bool> AssignModeratorAsync(
            Guid stationId,
            UpdateTeamAssignmentRequest.AssignModeratorRequest request,
            CancellationToken ct = default);
    }
}
