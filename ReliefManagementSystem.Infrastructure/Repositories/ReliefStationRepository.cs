using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    /// <summary>
    /// Concrete implementation of <see cref="IReliefStationRepository"/>.
    /// </summary>
    public class ReliefStationRepository : GenericRepository<ReliefStation>, IReliefStationRepository
    {
        public ReliefStationRepository(ApplicationDbContext context) : base(context) { }

        /// <inheritdoc/>
        public async Task<ReliefStation?> GetRegionalByLocationIdAsync(
            Guid regionLocationId,
            CancellationToken ct = default)
        {
            return await _context.ReliefStations
                .FirstOrDefaultAsync(
                    rs => rs.Level == ReliefStationLevel.Regional
                       && rs.LocationId == regionLocationId
                       && rs.IsActive,
                    ct);
        }

        /// <inheritdoc/>
        public IQueryable<ReliefStation> GetAllQueryable(
            ReliefStationLevel? level = null,
            string? search = null)
        {
            var query = _context.ReliefStations
                .Include(rs => rs.Location)
                .AsQueryable();

            if (level.HasValue)
                query = query.Where(rs => rs.Level == level.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(rs => rs.Name.Contains(search));

            return query.OrderByDescending(rs => rs.CreatedAt);
        }
    }
}
