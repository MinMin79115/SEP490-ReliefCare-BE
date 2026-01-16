using ReliefManagementSystem.Application.Features.Skill.Dtos;
using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Interface
{
    public interface ISkillService
    {
        Task<IReadOnlyList<SkillResponse>> GetAllSkillsAsync(CancellationToken cancellationToken);
        Task<SkillResponse?> GetSkillByIdAsync(Guid skillId, CancellationToken cancellationToken);
        Task<SkillResponse> CreateSkillAsync(CreateSkillRequest createSkillRequest,CancellationToken cancellationToken);
        Task UpdateSkillAsync(Guid skillId, UpdateSkillRequest request, CancellationToken cancellationToken);
        Task DeleteSkillAsync(Guid skillId, CancellationToken cancellationToken);
    }
}
