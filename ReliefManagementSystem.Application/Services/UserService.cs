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
using System.Threading;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public UserService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService   )
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
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

            if (profile == null)
                throw new InvalidOperationException("Volunteer profile not found.");

            profile.VerificationStatus = VerificationStatus.Approved;
            profile.VerifiedAt = DateTime.UtcNow;
            profile.VerifiedBy = _currentUserService.UserId;

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

        public async Task<VolunteerProfileResponse> AddNewSkillVolunteer(AddVolunteerRequest request,    CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var user = await _unitOfWork.VolunteerProfiles
                .GetByIdWithVolunteerProfileAsync(userId);

            if (user == null || user.VolunteerProfile == null)
                throw new InvalidOperationException(
                    "User not found or does not have a volunteer profile.");

            var profile = user.VolunteerProfile;

            var existingSkillIds = profile.VolunteerSkills
                .Select(vs => vs.SkillId)
                .ToHashSet();

            var newSkills = request.SkillIds
                .Where(skillId => !existingSkillIds.Contains(skillId))
                .Select(skillId => new VolunteerSkill
                {
                    VolunteerProfileId = profile.VolunteerProfileId,
                    SkillId = skillId
                })
                .ToList();

            if (!newSkills.Any())
                throw new InvalidOperationException("All skills already exist.");

            foreach (var skill in newSkills)
            {
                profile.VolunteerSkills.Add(skill);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new VolunteerProfileResponse
            {
                VolunteerProfileId = profile.VolunteerProfileId,
                FullName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Descriptions = profile.Descriptions,
                VerificationStatus = profile.VerificationStatus,
                Skills = profile.VolunteerSkills
                    .Select(vs => vs.SkillId)
                    .ToList()
            };
        }

        public async Task<VolunteerProfileResponse> RemoveSkillVolunteer(RemoveVolunteerSkillRequest request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var user = await _unitOfWork.VolunteerProfiles
                .GetByIdWithVolunteerProfileAsync(userId);

            if (user == null || user.VolunteerProfile == null)
                throw new InvalidOperationException(
                    "User not found or does not have a volunteer profile.");

            var profile = user.VolunteerProfile;

            var skillsToRemove = profile.VolunteerSkills
                .Where(vs => request.SkillIds.Contains(vs.SkillId))
                .ToList();

            if (!skillsToRemove.Any())
                throw new InvalidOperationException("No matching skills found to remove.");

            foreach (var skill in skillsToRemove)
            {
                profile.VolunteerSkills.Remove(skill);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new VolunteerProfileResponse
            {
                VolunteerProfileId = profile.VolunteerProfileId,
                FullName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Descriptions = profile.Descriptions,
                VerificationStatus = profile.VerificationStatus,
                Skills = profile.VolunteerSkills
                    .Select(vs => vs.SkillId)
                    .ToList()
            };
        }

        public async Task<List<VolunteerSkillResponse>> GetAllSkillsOfVolunteerAsync(CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var user = await _unitOfWork.VolunteerProfiles
                .GetByIdWithVolunteerProfileAsync(userId);

            if (user == null || user.VolunteerProfile == null)
                throw new InvalidOperationException(
                    "User not found or does not have a volunteer profile.");

            var skills = user.VolunteerProfile.VolunteerSkills
                .Select(vs => new VolunteerSkillResponse
                {
                    SkillId = vs.Skill.SkillId,
                    Code = vs.Skill.Code,
                    Name = vs.Skill.Name,
                    Description = vs.Skill.Description
                })
                .ToList();

            return skills;
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
