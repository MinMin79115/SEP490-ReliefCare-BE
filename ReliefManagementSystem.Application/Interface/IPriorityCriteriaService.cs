using ReliefManagementSystem.Application.Features.PriorityCriteria.DTOs.Request;
using ReliefManagementSystem.Application.Features.PriorityCriteria.DTOs.Response;
using ReliefManagementSystem.Application.Common.Models;
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
        Task<Pagination<PriorityCriteriaResponse>> GetAllAsync(SearchPriorityCriteriaRequest request, CancellationToken cancellationToken);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
