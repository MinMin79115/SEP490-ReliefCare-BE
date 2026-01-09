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

        // Include: User.VolunteerProfile.VolunteerSkills.Skill
        Task<List<TeamMember>> GetByTeamIdWithSkillsAsync(Guid teamId);

        Task<List<TeamMember>> GetByUserIdAsync(Guid userId);

        Task<bool> IsMemberAsync(Guid teamId, Guid userId);

        Task AddAsync(TeamMember teamMember);

        Task UpdateAsync(TeamMember teamMember);

        Task DeleteAsync(TeamMember teamMember);
    }
}
