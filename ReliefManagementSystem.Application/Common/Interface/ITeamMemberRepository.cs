using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ITeamMemberRepository
    {
        Task<TeamMember?> GetByTeamAndUserAsync(Guid teamId, Guid userId);

        Task<List<TeamMember>> GetByTeamIdWithSkillsAsync(Guid teamId);
        Task<TeamMember?> GetByTeamAndUserWithSkillsAsync(Guid teamId, Guid userId);
        Task<TeamMember?> GetTeamByUserIdAsync(Guid userId);

        Task<List<TeamMember>> GetByUserIdAsync(Guid userId);

        Task<bool> IsMemberAsync(Guid teamId, Guid userId);
        
        Task<TeamMember?> GetMemberAsync(Guid teamId, Guid userId);

        IQueryable<TeamMember> GetQueryable();

        Task AddAsync(TeamMember teamMember);
        Task UpdateAsync(TeamMember teamMember);
        Task DeleteAsync(TeamMember teamMember);
    }
}
