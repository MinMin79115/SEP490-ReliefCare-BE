using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ITeamRepository
    {
        Task<Team?> GetByIdAsync(Guid id);

        // Include: Moderator, Leader, TeamMembers.User.VolunteerProfile.VolunteerSkills.Skill
        Task<Team?> GetByIdWithDetailsAsync(Guid id);

        Task<List<Team>> GetAllAsync();

        Task<List<Team>> GetByModeratorIdAsync(Guid moderatorId);

        IQueryable<Team> GetQueryable();

        Task AddAsync(Team team);

        Task UpdateAsync(Team team);

        Task DeleteAsync(Team team);

        Task<bool> IsModeratorOfTeamAsync(Guid teamId, Guid userId);

        Task<bool> ExistsAsync(Guid id);

        Task<int> GetTeamMemberCountAsync(Guid teamId, CancellationToken cancellationToken = default);
    }
}
