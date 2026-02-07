using ReliefManagementSystem.Application.Features.VolunteerRequest.Request;
using ReliefManagementSystem.Application.Features.VolunteerRequest.Response;
using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IUserService
    {
        public Task<VolunteerProfileResponse> CreateVolunteerProfileAsync(
            CreateVolunteerRequest request,
            CancellationToken cancellationToken = default);
        Task<VolunteerProfileResponse> ApproveVolunteerProfileAsync(
           Guid volunteerProfileId,
           CancellationToken cancellationToken = default);

        Task<VolunteerProfileResponse> RejectVolunteerProfileAsync(
            Guid volunteerProfileId,
            string reason,
            CancellationToken cancellationToken = default);

        Task<VolunteerProfileResponse?> GetVolunteerProfileByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<List<VolunteerProfileResponse>> GetAllVolunteerProfilesAsync(
            CancellationToken cancellationToken = default);

        Task<VolunteerProfileResponse> AddNewSkillVolunteer(AddVolunteerRequest request, CancellationToken cancellationToken);

        Task<VolunteerProfileResponse> RemoveSkillVolunteer(RemoveVolunteerSkillRequest request, CancellationToken cancellationToken);

        Task<List<VolunteerSkillResponse>> GetAllSkillsOfVolunteerAsync(CancellationToken cancellationToken);
    }
}
