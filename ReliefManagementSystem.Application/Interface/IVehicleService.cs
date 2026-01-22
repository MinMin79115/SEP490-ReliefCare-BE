using ReliefManagementSystem.Application.Features.Vehicle.DTOs.Request;
using ReliefManagementSystem.Application.Features.Vehicle.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IVehicleService
    {
        Task<VehicleResponse> CreateVehicleAsync(CreateVehicleRequest request, Guid userId, CancellationToken cancellationToken = default);
        Task<VehicleResponse> GetVehicleByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<VehicleResponse>> GetAllVehiclesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<VehicleResponse>> GetVehiclesByStatusAsync(int status, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<VehicleResponse>> GetMyVehiclesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<VehicleResponse> UpdateVehicleAsync(Guid id, UpdateVehicleRequest request, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> DeleteVehicleAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    }
}
