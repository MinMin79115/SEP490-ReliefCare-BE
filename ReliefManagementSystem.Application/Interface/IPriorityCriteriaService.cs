using ReliefManagementSystem.Application.Features.PriorityCriteria.DTOs.Request;
using ReliefManagementSystem.Application.Features.PriorityCriteria.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IPriorityCriteriaService
    {
        Task<PriorityCriteriaResponse> CreateAsync(CreatePriorityCriteriaRequest request, CancellationToken cancellationToken);
        Task<PriorityCriteriaResponse> UpdateAsync(Guid id, UpdatePriorityCriteriaRequest request, CancellationToken cancellationToken);
        Task<PriorityCriteriaResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<List<PriorityCriteriaResponse>> GetAllAsync(CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
