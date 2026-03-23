using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        IQueryable<Vehicle> GetQueryable();
        Task<IReadOnlyList<Vehicle>> GetAllActiveAsync();
        Task<Vehicle?> GetByIdWithDetailsAsync(Guid id);
        Task<IReadOnlyList<Vehicle>> GetByStatusAsync(VehicleStatus status);
        Task<Vehicle?> GetByLicensePlateAsync(string licensePlate);
        Task<IReadOnlyList<Vehicle>> GetByCreatorAsync(Guid creatorId);
        //Task<bool> IsLicensePlateExistsAsync(string licensePlate, Guid? excludeId = null);
    }
}
