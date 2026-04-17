using ReliefManagementSystem.Application.Features.Vehicle.DTOs.Request;
using ReliefManagementSystem.Application.Common.Models;
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
        Task<VehicleResponse> CreateVehicleAsync(CreateVehicleRequest request, Guid userId, bool isManager, bool isModerator, CancellationToken cancellationToken = default);
        Task<VehicleResponse> GetVehicleByIdAsync(Guid id, Guid userId, bool isManager, bool isModerator, CancellationToken cancellationToken = default);
        Task<Pagination<VehicleResponse>> GetAllVehiclesAsync(SearchVehicleRequest request, Guid userId, bool isManager, bool isModerator, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<VehicleResponse>> GetVehiclesByStatusAsync(int status, Guid userId, bool isManager, bool isModerator, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<VehicleResponse>> GetMyVehiclesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<VehicleResponse> UpdateVehicleAsync(Guid id, UpdateVehicleRequest request, Guid userId, bool isManager, bool isModerator, CancellationToken cancellationToken = default);
        Task<bool> DeleteVehicleAsync(Guid id, Guid userId, bool isManager, bool isModerator, CancellationToken cancellationToken = default);
        Task<VehicleResponse> AssignVehicleToStationAsync(Guid vehicleId, Guid stationId, Guid userId, bool isManager, CancellationToken cancellationToken = default);
        Task<VehicleResponse> AssignVehicleToTeamAsync(Guid vehicleId, Guid teamId, Guid userId, CancellationToken cancellationToken = default);
        Task<object> GetVehicleCountsAsync(Guid? stationId, CancellationToken cancellationToken = default);
    }
}
