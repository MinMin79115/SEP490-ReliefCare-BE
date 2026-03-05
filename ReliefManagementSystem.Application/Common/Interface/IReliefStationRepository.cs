using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IReliefStationRepository : IGenericRepository<ReliefStation>
    {
        /// <summary>
        /// Tìm trạm Regional (cấp vùng) có LocationId khớp với vùng phụ trách
        /// của Manager (dùng để tự động gán ParentReliefStationId khi tạo trạm tỉnh).
        /// </summary>
        Task<ReliefStation?> GetRegionalByLocationIdAsync(
            Guid regionLocationId,
            CancellationToken ct = default);

        /// <summary>
        /// Trả về IQueryable tất cả trạm, có thể filter theo Level và search theo tên.
        /// Include Location navigation property để lấy LocationName.
        /// </summary>
        IQueryable<ReliefStation> GetAllQueryable(
            ReliefStationLevel? level = null,
            string? search = null);
    }
}
