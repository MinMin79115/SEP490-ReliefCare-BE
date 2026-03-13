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
        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.ReliefStations.AnyAsync(x => x.Name == name);
        }
        public async Task<bool> ExistsProvincialStationInLocationAsync(Guid locationId)
        {
            return await _context.ReliefStations.AnyAsync(x =>
                x.LocationId == locationId &&
                x.Level == ReliefStationLevel.Provincial);
        }

        public async Task<bool> ExistsByNameExcludingIdAsync(string name, Guid excludeStationId)
        {
            return await _context.ReliefStations.AnyAsync(x =>
                x.Name == name &&
                x.ReliefStationId != excludeStationId);
        }

        public async Task<(List<ReliefStation> Items, int TotalCount)> GetProvincialStationsAsync(
            string? search, int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            var query = _context.ReliefStations
                .Include(x => x.Location)
                .Where(x => x.Level == ReliefStationLevel.Provincial);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Name.Contains(search));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

    }
}
