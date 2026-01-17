using Microsoft.AspNetCore.Identity;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.VolunteerRequest.Request;
using ReliefManagementSystem.Application.Features.VolunteerRequest.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;


        public UserService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<VolunteerProfileResponse> CreateVolunteerProfileAsync(CreateVolunteerRequest request, CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId;
            var user = await _unitOfWork.Users.GetByIdWithVolunteerProfileAsync(userId);

            if (user == null || user.VolunteerProfile != null)
                throw new InvalidOperationException("User not found or already has a volunteer profile.");

            //if(user.DisplayName == null || user.PhoneNumber == null)
            //    throw new InvalidOperationException("User must have a display name and phone number to create a volunteer profile.");

            var volunteerProfile = new VolunteerProfile
            {
                UserId = userId,
                VerifiedAt = null,
                VerifiedBy = null,
                Descriptions = request.Descriptions,
                VolunteerSkills = request.SkillIds.Select(skillId => new VolunteerSkill
                {
                    SkillId = skillId
                }).ToList()
            };
            await _unitOfWork.VolunteerProfiles.AddAsync(volunteerProfile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new VolunteerProfileResponse
            {
                VolunteerProfileId = volunteerProfile.VolunteerProfileId,
                FullName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Descriptions = volunteerProfile.Descriptions,
                VerificationStatus = volunteerProfile.VerificationStatus,
                Skills = volunteerProfile.VolunteerSkills.Select(vs => vs.SkillId).ToList()
            };
        }

        public async Task<VolunteerProfileResponse> ApproveVolunteerProfileAsync(
           Guid volunteerProfileId,
           CancellationToken cancellationToken = default)
        {
            var profile = await _unitOfWork.VolunteerProfiles
                .GetByIdWithSkillsAndUserAsync(volunteerProfileId);
            var user = await _unitOfWork.Users.GetByIdAsync(profile.UserId);
            if (profile == null)
                throw new InvalidOperationException("Volunteer profile not found.");
            if( user == null)
                throw new InvalidOperationException("User not found.");

            profile.VerificationStatus = VerificationStatus.Approved;
            profile.VerifiedAt = DateTime.UtcNow;
            profile.VerifiedBy = _currentUserService.UserId;

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Contains(Role.User.ToString()))
            {
                await _userManager.RemoveFromRoleAsync(user, Role.User.ToString());
            }

            if (!currentRoles.Contains(Role.Volunteer.ToString()))
            {
                await _userManager.AddToRoleAsync(user, Role.Volunteer.ToString());
            }


            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(profile, profile.User);
        }


        public async Task<VolunteerProfileResponse> RejectVolunteerProfileAsync(
            Guid volunteerProfileId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            var profile = await _unitOfWork.VolunteerProfiles
                .GetByIdWithSkillsAndUserAsync(volunteerProfileId);

            if (profile == null)
                throw new InvalidOperationException("Volunteer profile not found.");

            profile.VerificationStatus = VerificationStatus.Rejected;
            profile.VerifiedAt = DateTime.UtcNow;
            profile.VerifiedBy = _currentUserService.UserId;
            profile.Reason = reason;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(profile, profile.User);
        }

        public async Task<VolunteerProfileResponse?> GetVolunteerProfileByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var profile = await _unitOfWork.VolunteerProfiles
                .GetByUserIdWithSkillsAsync(userId);

            if (profile == null)
                return null;

            return MapToResponse(profile, profile.User);
        }

        public async Task<List<VolunteerProfileResponse>> GetAllVolunteerProfilesAsync(
            CancellationToken cancellationToken = default)
        {
            var profiles = await _unitOfWork.VolunteerProfiles
                .GetAllWithSkillsAsync();

            return profiles
                .Select(profile => MapToResponse(profile, profile.User))
                .ToList();
        }

        private static VolunteerProfileResponse MapToResponse(
           VolunteerProfile profile,
           ApplicationUser? user)
        {
            return new VolunteerProfileResponse
            {
                VolunteerProfileId = profile.VolunteerProfileId,
                FullName = user?.DisplayName,
                Email = user?.Email,
                PhoneNumber = user?.PhoneNumber,
                Descriptions = profile.Descriptions,
                VerificationStatus = profile.VerificationStatus,
                Skills = profile.VolunteerSkills
                    .Select(vs => vs.SkillId)
                    .ToList()
            };
        }
    }
}
