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
        public ReliefStationRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ReliefStation>> GetAllWithDetailsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.Status != RelifeStationStatus.Closed)
                .Include(s => s.Manager)
                .Include(s => s.Location)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ReliefStation?> GetByIdWithDetailsAsync(
            Guid stationId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(s => s.Manager)
                .Include(s => s.Location)
                .Include(s => s.ReliefStations)
                    .ThenInclude(rst => rst.Team)
                .Include(s => s.Inventories)
                .FirstOrDefaultAsync(s => s.ReliefStationId == stationId, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ReliefStation>> GetByStatusAsync(
            RelifeStationStatus status,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.Status == status)
                .Include(s => s.Manager)
                .Include(s => s.Location)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ReliefStation>> GetByManagerIdAsync(
            Guid managerId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.ManagerId == managerId)
                .Include(s => s.Location)
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> IsNameExistsAsync(
            string name,
            Guid? excludeId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(s =>
                s.Name.ToLower() == name.ToLower() &&
                s.Status != RelifeStationStatus.Closed);

            if (excludeId.HasValue)
                query = query.Where(s => s.ReliefStationId != excludeId.Value);

            return await query.AnyAsync(cancellationToken);
        }
    }
}
