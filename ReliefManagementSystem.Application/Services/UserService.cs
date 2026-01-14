using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.VolunteerRequest.Request;
using ReliefManagementSystem.Application.Features.VolunteerRequest.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
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
    }
}
