using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class LocationRepository : GenericRepository<Location>, ILocationRepository
    {
        public LocationRepository(ApplicationDbContext context)
            : base(context)
        {

        }
        public async Task<List<Location>> GetByLevelAsync(LocationLevel level)
        {
            return await _context.Locations
                .Where(x => x.Level == level && x.Status == 1)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<List<Location>> GetChildrenByParentAsync(Guid parentId, LocationLevel level)
        {
            return await _context.Locations
                .Where(x => x.ParentId == parentId
                            && x.Level == level
                            && x.Status == 1)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
        public async Task<List<Location>> SearchByPathAsync(string path)
        {
            if (!path.EndsWith("/"))
                path += "/";

            return await _context.Locations
                .Where(x => x.Path.StartsWith(path) && x.Status == 1)
                .OrderBy(x => x.Path)
                .ToListAsync();
        }

        public async Task<List<Location>> GetAllActiveAsync()
        {
            return await _context.Locations
                .Where(x => x.Status == 1)
                .OrderBy(x => x.Path)
                .ToListAsync();
        }

        public async Task<string?> GetFullNameByLocationId(Guid locationId)
        {
            return await _context.Locations
                .Where(l => l.LocationId == locationId)
                .Select(l => l.FullName)
                .FirstOrDefaultAsync();
        }
    }
}
