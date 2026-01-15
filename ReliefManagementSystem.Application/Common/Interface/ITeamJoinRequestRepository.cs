using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ITeamJoinRequestRepository : IGenericRepository<TeamJoinRequest>
    {

        // Include: Team.Moderator, Volunteer.VolunteerProfile.VolunteerSkills.Skill, Reviewer
        Task<TeamJoinRequest?> GetByIdWithDetailsAsync(Guid id);

        Task<List<TeamJoinRequest>> GetByVolunteerIdWithDetailsAsync(Guid volunteerId);

        Task<List<TeamJoinRequest>> GetByTeamIdWithDetailsAsync(Guid teamId);

        Task<List<TeamJoinRequest>> GetPendingRequestsByModeratorWithDetailsAsync(Guid moderatorId);

        Task<TeamJoinRequest?> GetExistingPendingRequestAsync(Guid teamId, Guid volunteerId);

    }
}
