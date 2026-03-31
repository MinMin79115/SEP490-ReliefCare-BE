using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Exceptions.Auth;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.User;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IImageService _imageService;


        public UserService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, IImageService imageService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _userManager = userManager;
            _roleManager = roleManager;
            _imageService = imageService;
        }

        /// <summary>
        /// Lấy profile của user đang đăng nhập.
        /// Throw UserNotFoundException nếu user không tồn tại.
        /// </summary>
        public async Task<UserProfileResponse> GetProfileAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId
                         ?? throw new UnauthorizedAccessException("User not authenticated"); 
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
                throw new UserNotFoundException(userId.ToString());

            var roles = await _userManager.GetRolesAsync(user);

            return new UserProfileResponse
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                PictureUrl = user.PictureUrl,
                BanReason = user.BanReason,
                IsBanned = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = user.LockoutEnd,
                Roles = roles.ToList()
            };
        }

        /// <summary>
        /// Lấy danh sách tất cả users có phân trang (dành cho Admin).
        /// Trả về Pagination&lt;UserProfileResponse&gt; với thông tin phân trang.
        /// </summary>
        public async Task<Pagination<UserProfileResponse>> GetAllProfilesAsync(
            GetAllUsersRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.Users.GetAllUsersQueryable();

            var normalizedRole = string.IsNullOrWhiteSpace(request.Role)
                ? null
                : request.Role.Trim();

            if (!string.IsNullOrWhiteSpace(normalizedRole))
            {
                var roleExists = await _roleManager.RoleExistsAsync(normalizedRole);
                if (!roleExists)        
                {
                    return new Pagination<UserProfileResponse>(
                        new List<UserProfileResponse>(),
                        0,
                        request.PageIndex,
                        request.PageSize);
                }

                var usersInRole = await _userManager.GetUsersInRoleAsync(normalizedRole);
                var userIdsInRole = usersInRole.Select(u => u.Id).ToHashSet();
                query = query.Where(u => userIdsInRole.Contains(u.Id));
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();

                query = query.Where(u =>
                    (u.DisplayName ?? string.Empty).Contains(keyword) ||
                    (u.Email ?? string.Empty).Contains(keyword) ||
                    (u.PhoneNumber ?? string.Empty).Contains(keyword)
                );
            }

            if (request.IsBanned.HasValue)
            {
                var now = DateTimeOffset.UtcNow;

                if (request.IsBanned.Value)
                {
                    query = query.Where(u =>
                        u.LockoutEnabled &&
                        u.LockoutEnd.HasValue &&
                        u.LockoutEnd > now);
                }
                else
                {
                    query = query.Where(u =>
                        !u.LockoutEnabled ||
                        !u.LockoutEnd.HasValue ||
                        u.LockoutEnd <= now);
                }
            }

            var pagedUsers = await Pagination<ApplicationUser>.ToPagedList(
                query, request.PageIndex, request.PageSize);

            var userResponses = new List<UserProfileResponse>();
            foreach (var user in pagedUsers.Items!)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userResponses.Add(new UserProfileResponse
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    DateOfBirth = user.DateOfBirth,
                    Gender = user.Gender,
                    PictureUrl = user.PictureUrl,
                    BanReason = user.BanReason,
                    IsBanned = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                    LockoutEnd = user.LockoutEnd,
                    Roles = roles.ToList()
                });
            }

            return new Pagination<UserProfileResponse>(
                userResponses,
                pagedUsers.TotalCount,
                pagedUsers.CurrentPage,
                pagedUsers.PageSize);
        }

        public async Task<Pagination<ModeratorProfileResponse>> GetModeratorsAsync(
            GetModeratorsRequest request,
            CancellationToken cancellationToken = default)
        {
            var moderators = await _userManager.GetUsersInRoleAsync(Role.Moderator.ToString());
            var now = DateTimeOffset.UtcNow;
            var query = moderators.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                query = query.Where(u =>
                    (u.DisplayName ?? string.Empty).Contains(keyword) ||
                    (u.Email ?? string.Empty).Contains(keyword) ||
                    (u.PhoneNumber ?? string.Empty).Contains(keyword));
            }

            if (request.IsBanned.HasValue)
            {
                if (request.IsBanned.Value)
                {
                    query = query.Where(u =>
                        u.LockoutEnabled &&
                        u.LockoutEnd.HasValue &&
                        u.LockoutEnd > now);
                }
                else
                {
                    query = query.Where(u =>
                        !u.LockoutEnabled ||
                        !u.LockoutEnd.HasValue ||
                        u.LockoutEnd <= now);
                }
            }

            var pagedModerators = await Pagination<ApplicationUser>.ToPagedList(
                query,
                request.PageIndex,
                request.PageSize);

            var responses = new List<ModeratorProfileResponse>();

            foreach (var moderator in pagedModerators.Items ?? new List<ApplicationUser>())
            {
                var moderatorProfile = await _unitOfWork.ModeratorProfiles
                    .GetByUserIdAsync(moderator.Id, cancellationToken);

                responses.Add(new ModeratorProfileResponse
                {
                    Id = moderator.Id,
                    DisplayName = moderator.DisplayName,
                    Email = moderator.Email,
                    PhoneNumber = moderator.PhoneNumber,
                    PictureUrl = moderator.PictureUrl,
                    IsBanned = moderator.LockoutEnabled && moderator.LockoutEnd.HasValue && moderator.LockoutEnd > now,
                    LockoutEnd = moderator.LockoutEnd,
                    BanReason = moderator.BanReason,
                    ModeratorStatus = moderatorProfile?.Status,
                    IsStationHead = moderatorProfile?.IsStationHead ?? false,
                    IsManagingStation = moderatorProfile?.ReliefStationId != null,
                    ReliefStationId = moderatorProfile?.ReliefStationId,
                    ReliefStationName = moderatorProfile?.ReliefStation?.Name
                });
            }

            return new Pagination<ModeratorProfileResponse>(
                responses,
                pagedModerators.TotalCount,
                pagedModerators.CurrentPage,
                pagedModerators.PageSize);
        }

        public async Task<VolunteerProfileResponse> CreateVolunteerProfileAsync(CreateVolunteerRequest request, CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId
                         ?? throw new UnauthorizedAccessException("User not authenticated"); 
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
                YearsOfExperience = request.YearsOfExperience,
                PreferredTeamRole = request.PreferredTeamRole,
                Status = VolunteerStatus.Inactive,
                VolunteerSkills = request.SkillIds.Select(skillId => new VolunteerSkill
                {
                    SkillId = skillId
                }).ToList(),
                Certificates = request.Certificates.Select(c => new VolunteerCertificate
                {
                    Name = c.Name,
                    IssuedBy = c.IssuedBy,
                    IssuedDate = c.IssuedDate,
                    ExpiryDate = c.ExpiryDate,
                    FileUrl = c.FileUrl
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
                YearsOfExperience = volunteerProfile.YearsOfExperience,
                PreferredTeamRole = volunteerProfile.PreferredTeamRole,
                Skills = volunteerProfile.VolunteerSkills.Select(vs => vs.SkillId).ToList(),
                Certificates = volunteerProfile.Certificates
                    .Select(c => new VolunteerCertificateResponse
                    {
                        Name = c.Name,
                        IssuedBy = c.IssuedBy,
                        IssuedDate = c.IssuedDate,
                        ExpiryDate = c.ExpiryDate,
                        FileUrl = c.FileUrl
                    }).ToList()
                    
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
            profile.Status = VolunteerStatus.Active; 

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
            profile.Status = VolunteerStatus.Inactive;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(profile, profile.User);
        }

        public async Task<VolunteerProfileResponse?> GetMyVolunteerProfileAsync(
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId
                         ?? throw new UnauthorizedAccessException("User not authenticated");

            var profile = await _unitOfWork.VolunteerProfiles
                .GetByUserIdWithSkillsAsync(userId);

            if (profile == null)
                return null;

            return MapToResponse(profile, profile.User);
        }

        public async Task<Pagination<VolunteerProfileResponse>> GetAllVolunteerProfilesAsync(
            SearchVolunteerProfilesRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.Users.GetQueryableWithVolunteerProfile()
                .Where(u => u.VolunteerProfile != null)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                query = query.Where(u =>
                    (u.DisplayName ?? string.Empty).Contains(keyword) ||
                    (u.Email ?? string.Empty).Contains(keyword) ||
                    (u.PhoneNumber ?? string.Empty).Contains(keyword));
            }

            query = query.OrderBy(u => u.DisplayName ?? u.Email ?? string.Empty);

            var pagedUsers = await Pagination<ApplicationUser>.ToPagedList(query, request.PageIndex, request.PageSize);

            var items = pagedUsers.Items!
                .Where(u => u.VolunteerProfile != null)
                .Select(u => MapToResponse(u.VolunteerProfile!, u))
                .ToList();

            return new Pagination<VolunteerProfileResponse>(
                items,
                pagedUsers.TotalCount,
                pagedUsers.CurrentPage,
                pagedUsers.PageSize);
        }

        public async Task<Pagination<VolunteerApplicationReviewResponse>> GetPendingVolunteerApplicationsAsync(
            GetPendingVolunteerApplicationsRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.VolunteerProfiles.GetQueryableForReview()
                .AsQueryable();

            if (request.VerificationStatus.HasValue)
            {
                query = query.Where(vp => vp.VerificationStatus == request.VerificationStatus.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                query = query.Where(vp =>
                    (vp.User != null && (
                        (vp.User.DisplayName ?? string.Empty).Contains(keyword) ||
                        (vp.User.Email ?? string.Empty).Contains(keyword) ||
                        (vp.User.PhoneNumber ?? string.Empty).Contains(keyword)
                    )));
            }

            query = query.OrderByDescending(vp => vp.CreatedAt);

            var paged = await Pagination<VolunteerProfile>.ToPagedList(
                query,
                request.PageIndex,
                request.PageSize);

            var items = paged.Items!.Select(vp => new VolunteerApplicationReviewResponse
            {
                VolunteerProfileId = vp.VolunteerProfileId,
                UserId = vp.UserId,
                FullName = vp.User?.DisplayName,
                Email = vp.User?.Email,
                PhoneNumber = vp.User?.PhoneNumber,
                Address = vp.User?.Address,
                DateOfBirth = vp.User?.DateOfBirth,
                Gender = vp.User?.Gender,
                AppliedAt = vp.CreatedAt,
                VerificationStatus = vp.VerificationStatus,
                Status = vp.Status,
                VerifiedBy = vp.VerifiedBy,
                VerifiedAt = vp.VerifiedAt,
                Reason = vp.Reason,
                Descriptions = vp.Descriptions,
                YearsOfExperience = vp.YearsOfExperience,
                PreferredTeamRole = vp.PreferredTeamRole,
                VolunteerType = vp.VolunteerType,
                Skills = vp.VolunteerSkills.Select(vs => new VolunteerSkillResponse
                {
                    SkillId = vs.SkillId,
                    Code = vs.Skill?.Code ?? string.Empty,
                    Name = vs.Skill?.Name ?? string.Empty,
                    Description = vs.Skill?.Description
                }).ToList(),
                Certificates = vp.Certificates.Select(c => new VolunteerCertificateResponse
                {
                    Name = c.Name,
                    IssuedBy = c.IssuedBy,
                    IssuedDate = c.IssuedDate,
                    ExpiryDate = c.ExpiryDate,
                    FileUrl = c.FileUrl
                }).ToList()
            }).ToList();

            return new Pagination<VolunteerApplicationReviewResponse>(
                items,
                paged.TotalCount,
                paged.CurrentPage,
                paged.PageSize);
        }

        public async Task<VolunteerProfileResponse> AddNewSkillVolunteer(AddVolunteerRequest request,    CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                         ?? throw new UnauthorizedAccessException("User not authenticated");
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
                PreferredTeamRole = profile.PreferredTeamRole,
                Skills = profile.VolunteerSkills
                    .Select(vs => vs.SkillId)
                    .ToList()
            };
        }

        public async Task<VolunteerProfileResponse> RemoveSkillVolunteer(RemoveVolunteerSkillRequest request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                         ?? throw new UnauthorizedAccessException("User not authenticated");
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
                PreferredTeamRole = profile.PreferredTeamRole,
                Skills = profile.VolunteerSkills
                    .Select(vs => vs.SkillId)
                    .ToList()
            };
        }

        public async Task<List<VolunteerSkillResponse>> GetAllSkillsOfVolunteerAsync(CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                         ?? throw new UnauthorizedAccessException("User not authenticated");
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


        /// <summary>
        /// Cập nhật profile user đang đăng nhập (partial update).
        /// Throw UserNotFoundException nếu user không tồn tại.
        /// </summary>
        public async Task<UserProfileResponse> UpdateUserProfileAsync(
            UpdateUserProfileRequest request,
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId
                         ?? throw new UnauthorizedAccessException("User not authenticated");
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
                throw new UserNotFoundException(userId.ToString());

            // Update only non-null fields (partial update)
            if (request.DisplayName != null)
                user.DisplayName = request.DisplayName;

            if (request.PhoneNumber != null)
                user.PhoneNumber = request.PhoneNumber;

            if (request.Address != null)
                user.Address = request.Address;

            if (request.DateOfBirth.HasValue)
                user.DateOfBirth = request.DateOfBirth.Value;

            if (request.Gender != null)
                user.Gender = request.Gender;

            // Handle avatar upload
            if (request.PictureUrl != null)
            {
                if (!string.IsNullOrEmpty(user.PicturePublicId) &&
                    !string.IsNullOrEmpty(request.PicturePublicId))
                {
                    await _imageService.DeleteImageAsync(user.PicturePublicId);
                }

                user.PictureUrl = request.PictureUrl;
                user.PicturePublicId = request.PicturePublicId;
            }

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var roles = await _userManager.GetRolesAsync(user);

            return new UserProfileResponse
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                PictureUrl = user.PictureUrl,
                BanReason = user.BanReason,
                IsBanned = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = user.LockoutEnd,
                Roles = roles.ToList()
            };
        }

        public async Task<UserProfileResponse> BanUserAsync(
            Guid userId,
            BanUserRequest request,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                       ?? throw new UserNotFoundException(userId.ToString());

            user.BanReason = request.Reason.Trim();
            user.LockoutEnabled = true;

            var lockResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            if (!lockResult.Succeeded)
            {
                var errors = lockResult.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
                throw new ReliefManagementSystem.Application.Common.Exceptions.ValidationException(errors);
            }

            await _userManager.UpdateSecurityStampAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            return new UserProfileResponse
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                PictureUrl = user.PictureUrl,
                BanReason = user.BanReason,
                IsBanned = true,
                LockoutEnd = user.LockoutEnd,
                Roles = roles.ToList()
            };
        }

        public async Task<UserProfileResponse> UnbanUserAsync(
            Guid userId,
            UnbanUserRequest request,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                       ?? throw new UserNotFoundException(userId.ToString());

            user.BanReason = request.Note;

            var unlockResult = await _userManager.SetLockoutEndDateAsync(user, null);
            if (!unlockResult.Succeeded)
            {
                var errors = unlockResult.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
                throw new ReliefManagementSystem.Application.Common.Exceptions.ValidationException(errors);
            }

            await _userManager.UpdateSecurityStampAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            return new UserProfileResponse
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                PictureUrl = user.PictureUrl,
                BanReason = user.BanReason,
                IsBanned = false,
                LockoutEnd = user.LockoutEnd,
                Roles = roles.ToList()
            };
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
                PreferredTeamRole = profile.PreferredTeamRole,
                Skills = profile.VolunteerSkills
                    .Select(vs => vs.SkillId)
                    .ToList()
            };
        }
    }
}
