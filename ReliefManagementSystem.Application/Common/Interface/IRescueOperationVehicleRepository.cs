using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IRescueOperationVehicleRepository : IGenericRepository<RescueOperationVehicle>
    {
        Task<List<RescueOperationVehicle>> GetByOperationIdAsync(Guid rescueOperationId, CancellationToken cancellationToken = default);
        Task ReplaceForOperationAsync(Guid rescueOperationId, List<RescueOperationVehicle> vehicles, CancellationToken cancellationToken = default);
    }
}
