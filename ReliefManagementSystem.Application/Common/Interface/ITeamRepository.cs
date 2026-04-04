using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        // Include: Moderator, Leader, TeamMembers.User.VolunteerProfile.VolunteerSkills.Skill
        Task<Team?> GetByIdWithDetailsAsync(Guid teamId);

        //hold to override
        Task<List<Team>> GetByModeratorIdAsync(Guid moderatorId);

        Task<List<Team>>GetTeamsByModeratorWithMembersAsync(Guid moderatorId);
        IQueryable<Team> GetQueryable();

        Task<bool> IsModeratorOfTeamAsync(Guid teamId, Guid userId);

        Task<int> GetTeamMemberCountAsync(Guid teamId, CancellationToken cancellationToken = default);
        Task<int> GetAvailablePeopleCountAsync(CancellationToken cancellationToken = default);
        Task<int> GetAvailablePeopleCountByTeamAsync(Guid teamId, CancellationToken cancellationToken = default);
    }
}
