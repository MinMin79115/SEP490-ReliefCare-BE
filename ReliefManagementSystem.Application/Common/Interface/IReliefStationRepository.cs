using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Request;
using ReliefManagementSystem.Application.Common.Models;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IReliefStationRepository : IGenericRepository<ReliefStation>
    {
        Task<bool> ExistsByNameAsync(string name);

        Task<bool> ExistsProvincialStationInLocationAsync(Guid locationId);

        /// <summary>Kiểm tra tồn tại trạm cùng tên, ngoại trừ trạm đang cập nhật (dùng khi update).</summary>
        Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeStationId);

        /// <summary>Lấy danh sách trạm cấp Tỉnh (Provincial), có hỗ trợ tìm kiếm và phân trang.</summary>
        Task<Pagination<ReliefStation>> GetProvincialStationsAsync(
            GetAllStationsRequest request,
            CancellationToken cancellationToken);
    }
}
