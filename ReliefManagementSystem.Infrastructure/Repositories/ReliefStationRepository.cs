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

    }
}
