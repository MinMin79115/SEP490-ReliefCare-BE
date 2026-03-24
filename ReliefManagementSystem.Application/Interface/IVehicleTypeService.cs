using ReliefManagementSystem.Application.Features.VehicleType.DTOs.Request;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.VehicleType.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IVehicleTypeService
    {
        Task<VehicleTypeResponse> CreateVehicleTypeAsync(CreateVehicleTypeRequest request, CancellationToken cancellationToken = default);
        Task<VehicleTypeDetailResponse> GetVehicleTypeByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Pagination<VehicleTypeResponse>> GetAllVehicleTypesAsync(SearchVehicleTypeRequest request, CancellationToken cancellationToken = default);
        Task<VehicleTypeResponse> UpdateVehicleTypeAsync(Guid id, UpdateVehicleTypeRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteVehicleTypeAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
