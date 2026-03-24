using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class VehicleTypeRepository : GenericRepository<VehicleType>, IVehicleTypeRepository
    {
        public VehicleTypeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IQueryable<VehicleType> GetQueryable()
        {
            return _dbSet
                .Where(vt => !vt.IsDeleted)
                .AsQueryable();
        }

        public async Task<IReadOnlyList<VehicleType>> GetAllActiveAsync()
        {
            return await _dbSet
                .Where(vt => !vt.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<VehicleType?> GetByIdWithVehiclesAsync(Guid id)
        {
            return await _dbSet
                .Include(vt => vt.Vehicles.Where(v => !v.IsDeleted))
                .FirstOrDefaultAsync(vt => vt.VehicleTypeId == id && !vt.IsDeleted);
        }

        public async Task<VehicleType?> GetByTypeNameAsync(string typeName)
        {
            return await _dbSet
                .FirstOrDefaultAsync(vt => vt.TypeName == typeName && !vt.IsDeleted);
        }

        public async Task<bool> IsTypeNameExistsAsync(string typeName, Guid? excludeId = null)
        {
            var query = _dbSet.Where(vt => vt.TypeName == typeName && !vt.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(vt => vt.VehicleTypeId != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
