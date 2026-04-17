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

        Task<Pagination<ModeratorProfileResponse>> GetModeratorsAsync(
            GetModeratorsRequest request,
            CancellationToken cancellationToken = default);

        Task<ModeratorProfileResponse> CreateModeratorAsync(
            CreateModeratorAccountRequest request,
            CancellationToken cancellationToken = default);

        Task<ModeratorProfileResponse> GetModeratorByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<ModeratorProfileResponse> UpdateModeratorAsync(
            Guid userId,
            UpdateModeratorAccountRequest request,
            CancellationToken cancellationToken = default);

        Task<ModeratorProfileResponse> SoftDeleteModeratorAsync(
            Guid userId,
            SoftDeletePrivilegedAccountRequest request,
            CancellationToken cancellationToken = default);

        Task<Pagination<ManagerProfileResponse>> GetManagersAsync(
            GetManagersRequest request,
            CancellationToken cancellationToken = default);

        Task<ManagerProfileResponse> CreateManagerAsync(
            CreateManagerAccountRequest request,
            CancellationToken cancellationToken = default);

        Task<ManagerProfileResponse> GetManagerByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<ManagerProfileResponse> UpdateManagerAsync(
            Guid userId,
            UpdateManagerAccountRequest request,
            CancellationToken cancellationToken = default);

        Task<ManagerProfileResponse> SoftDeleteManagerAsync(
            Guid userId,
            SoftDeletePrivilegedAccountRequest request,
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

        Task<VolunteerProfileResponse> ResubmitVolunteerProfileAsync(
            ResubmitVolunteerRequest request,
            CancellationToken cancellationToken = default);

        Task<Pagination<VolunteerProfileResponse>> GetAllVolunteerProfilesAsync(
            SearchVolunteerProfilesRequest request,
            CancellationToken cancellationToken = default);

        Task<Pagination<VolunteerProfileResponse>> GetUnassignedVolunteersAsync(
            SearchVolunteerProfilesRequest request,
            CancellationToken cancellationToken = default);

        Task<List<VolunteerProfileResponse>> GetAllUnassignedVolunteersListAsync(
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

        Task<UserProfileResponse> BanUserAsync(
            Guid userId,
            BanUserRequest request,
            CancellationToken cancellationToken = default);

        Task<UserProfileResponse> UnbanUserAsync(
            Guid userId,
            UnbanUserRequest request,
            CancellationToken cancellationToken = default);
    }
}
