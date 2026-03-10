using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.ReliefStation.Dtos;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Request;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Response;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Interface
{
    /// <summary>Service contract for ReliefStation and team assignment operations.</summary>
    public interface IReliefStationService
    {
        Task<Guid> CreateProvincialReliefStationAsync(CreateProvincialReliefStationRequest request,CancellationToken cancellationToken);
    }
}
