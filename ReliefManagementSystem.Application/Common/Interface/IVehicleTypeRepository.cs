using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IVehicleTypeRepository : IGenericRepository<VehicleType>
    {
        IQueryable<VehicleType> GetQueryable();
        Task<IReadOnlyList<VehicleType>> GetAllActiveAsync();
        Task<VehicleType?> GetByIdWithVehiclesAsync(Guid id);
        Task<VehicleType?> GetByTypeNameAsync(string typeName);
        Task<bool> IsTypeNameExistsAsync(string typeName, Guid? excludeId = null);
    }
}
