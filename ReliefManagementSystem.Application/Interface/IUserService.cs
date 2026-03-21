using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.User;
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
        /// <summary>
        /// Lấy profile của user đang đăng nhập
        /// </summary>
        Task<UserProfileResponse> GetProfileAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy danh sách tất cả users có phân trang (Admin)
        /// </summary>
        Task<Pagination<UserProfileResponse>> GetAllProfilesAsync(
            GetAllUsersRequest request,
            CancellationToken cancellationToken = default);

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

        Task<VolunteerProfileResponse?> GetMyVolunteerProfileAsync(
            CancellationToken cancellationToken = default);

        Task<List<VolunteerProfileResponse>> GetAllVolunteerProfilesAsync(
            CancellationToken cancellationToken = default);

        Task<Pagination<VolunteerApplicationReviewResponse>> GetPendingVolunteerApplicationsAsync(
            GetPendingVolunteerApplicationsRequest request,
            CancellationToken cancellationToken = default);

        Task<VolunteerProfileResponse> AddNewSkillVolunteer(AddVolunteerRequest request, CancellationToken cancellationToken);

        Task<VolunteerProfileResponse> RemoveSkillVolunteer(RemoveVolunteerSkillRequest request, CancellationToken cancellationToken);

        Task<List<VolunteerSkillResponse>> GetAllSkillsOfVolunteerAsync(CancellationToken cancellationToken);

        Task<UserProfileResponse> UpdateUserProfileAsync(
            UpdateUserProfileRequest request,
            CancellationToken cancellationToken = default);
    }
}
