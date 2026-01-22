using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Vehicle>> GetAllActiveAsync()
        {
            return await _dbSet
                .Include(v => v.VehicleType)
                .Include(v => v.Creator)
                .Where(v => !v.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Vehicle?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(v => v.VehicleType)
                .Include(v => v.Creator)
                .FirstOrDefaultAsync(v => v.VehicleId == id && !v.IsDeleted);
        }

        public async Task<IReadOnlyList<Vehicle>> GetByStatusAsync(VehicleStatus status)
        {
            return await _dbSet
                .Include(v => v.VehicleType)
                .Include(v => v.Creator)
                .Where(v => v.Status == status && !v.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Vehicle?> GetByLicensePlateAsync(string licensePlate)
        {
            return await _dbSet
                .Include(v => v.VehicleType)
                .Include(v => v.Creator)
                .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate && !v.IsDeleted);
        }

        public async Task<IReadOnlyList<Vehicle>> GetByCreatorAsync(Guid creatorId)
        {
            return await _dbSet
                .Include(v => v.VehicleType)
                .Where(v => v.CreatedBy == creatorId && !v.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> IsLicensePlateExistsAsync(string licensePlate, Guid? excludeId = null)
        {
            var query = _dbSet.Where(v => v.LicensePlate == licensePlate && !v.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(v => v.VehicleId != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
