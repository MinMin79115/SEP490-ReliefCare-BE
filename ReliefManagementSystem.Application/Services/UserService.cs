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
        private readonly IEmailService _emailService;
        private readonly ICampaignService _campaignService;


        public UserService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IImageService imageService,
            IEmailService emailService,
            ICampaignService campaignService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _userManager = userManager;
            _roleManager = roleManager;
            _imageService = imageService;
            _emailService = emailService;
            _campaignService = campaignService;
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
            var moderatorIds = moderators.Select(u => u.Id).ToHashSet();
            var query = _unitOfWork.Users
                .GetAllUsersQueryable()
                .Where(u => moderatorIds.Contains(u.Id));

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

                responses.Add(MapToModeratorProfileResponse(moderator, moderatorProfile));
            }

            return new Pagination<ModeratorProfileResponse>(
                responses,
                pagedModerators.TotalCount,
                pagedModerators.CurrentPage,
                pagedModerators.PageSize);
        }

        public async Task<ModeratorProfileResponse> CreateModeratorAsync(
            CreateModeratorAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureUniqueCredentialsAsync(request.Email, request.UserName, request.PhoneNumber, cancellationToken);

            if (request.ReliefStationId.HasValue)
            {
                var station = await _unitOfWork.ReliefStations.GetByIdAsync(request.ReliefStationId.Value);
                if (station == null)
                    throw new InvalidOperationException("Relief station not found.");

                if (request.IsStationHead)
                {
                    await EnsureStationHeadAvailableAsync(request.ReliefStationId.Value, null, cancellationToken);
                }
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = request.Email.Trim(),
                NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
                UserName = request.UserName.Trim(),
                NormalizedUserName = request.UserName.Trim().ToUpperInvariant(),
                PhoneNumber = request.PhoneNumber.Trim(),
                DisplayName = request.FullName.Trim(),
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            EnsureIdentitySucceeded(createResult);

            var roleResult = await _userManager.AddToRoleAsync(user, Role.Moderator.ToString());
            EnsureIdentitySucceeded(roleResult);

            var profile = new ModeratorProfile
            {
                ModeratorProfileId = Guid.NewGuid(),
                UserId = user.Id,
                ReliefStationId = request.ReliefStationId,
                IsStationHead = request.IsStationHead,
                AppointedAt = DateTime.UtcNow,
                Notes = request.Notes?.Trim(),
                Status = request.Status,
                StatusReason = request.StatusReason?.Trim()
            };

            await _unitOfWork.ModeratorProfiles.AddAsync(profile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var createdUser = await _unitOfWork.Users.GetByIdWithModeratorProfileAsync(user.Id, cancellationToken)
                ?? throw new InvalidOperationException("Created moderator not found.");

            return MapToModeratorProfileResponse(createdUser, createdUser.ModeratorProfile);
        }

        public async Task<ModeratorProfileResponse> GetModeratorByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByIdWithModeratorProfileAsync(userId, cancellationToken)
                ?? throw new UserNotFoundException(userId.ToString());

            await EnsureUserInRoleAsync(user, Role.Moderator);
            return MapToModeratorProfileResponse(user, user.ModeratorProfile);
        }

        public async Task<ModeratorProfileResponse> UpdateModeratorAsync(
            Guid userId,
            UpdateModeratorAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByIdWithModeratorProfileAsync(userId, cancellationToken)
                ?? throw new UserNotFoundException(userId.ToString());

            await EnsureUserInRoleAsync(user, Role.Moderator);
            var profile = user.ModeratorProfile ?? throw new InvalidOperationException("Moderator profile not found.");

            await ApplyBasicUserUpdatesAsync(user, request.Email, request.UserName, request.PhoneNumber, request.FullName, cancellationToken);

            if (request.ClearReliefStation)
            {
                profile.ReliefStationId = null;
                if (!request.IsStationHead.HasValue)
                {
                    profile.IsStationHead = false;
                }
            }
            else if (request.ReliefStationId.HasValue)
            {
                var station = await _unitOfWork.ReliefStations.GetByIdAsync(request.ReliefStationId.Value);
                if (station == null)
                    throw new InvalidOperationException("Relief station not found.");

                var willBeStationHead = request.IsStationHead ?? profile.IsStationHead;
                if (willBeStationHead)
                {
                    await EnsureStationHeadAvailableAsync(request.ReliefStationId.Value, user.Id, cancellationToken);
                }

                profile.ReliefStationId = request.ReliefStationId.Value;
            }

            if (request.IsStationHead.HasValue)
            {
                if (request.IsStationHead.Value && profile.ReliefStationId.HasValue)
                {
                    await EnsureStationHeadAvailableAsync(profile.ReliefStationId.Value, user.Id, cancellationToken);
                }

                profile.IsStationHead = request.IsStationHead.Value;
            }

            if (request.Notes != null)
                profile.Notes = request.Notes.Trim();

            if (request.Status.HasValue)
                profile.Status = request.Status.Value;

            if (request.StatusReason != null)
                profile.StatusReason = request.StatusReason.Trim();

            var updateResult = await _userManager.UpdateAsync(user);
            EnsureIdentitySucceeded(updateResult);
            await _unitOfWork.ModeratorProfiles.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedUser = await _unitOfWork.Users.GetByIdWithModeratorProfileAsync(user.Id, cancellationToken)
                ?? throw new InvalidOperationException("Updated moderator not found.");

            return MapToModeratorProfileResponse(updatedUser, updatedUser.ModeratorProfile);
        }

        public async Task<ModeratorProfileResponse> SoftDeleteModeratorAsync(
            Guid userId,
            SoftDeletePrivilegedAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByIdWithModeratorProfileAsync(userId, cancellationToken)
                ?? throw new UserNotFoundException(userId.ToString());

            await EnsureUserInRoleAsync(user, Role.Moderator);

            if (user.ModeratorProfile != null)
            {
                user.ModeratorProfile.Status = ModeratorStatus.Dismissed;
                user.ModeratorProfile.StatusReason = request.Reason.Trim();
                user.ModeratorProfile.IsStationHead = false;
                await _unitOfWork.ModeratorProfiles.UpdateAsync(user.ModeratorProfile);
            }

            await BanUserAsync(userId, new BanUserRequest { Reason = request.Reason }, cancellationToken);

            var updatedUser = await _unitOfWork.Users.GetByIdWithModeratorProfileAsync(userId, cancellationToken)
                ?? throw new InvalidOperationException("Moderator not found after soft delete.");

            return MapToModeratorProfileResponse(updatedUser, updatedUser.ModeratorProfile);
        }

        public async Task<Pagination<ManagerProfileResponse>> GetManagersAsync(
            GetManagersRequest request,
            CancellationToken cancellationToken = default)
        {
            var managers = await _userManager.GetUsersInRoleAsync(Role.Manager.ToString());
            var now = DateTimeOffset.UtcNow;
            var managerIds = managers.Select(u => u.Id).ToHashSet();
            var query = _unitOfWork.Users
                .GetAllUsersQueryable()
                .Where(u => managerIds.Contains(u.Id));

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

            var pagedManagers = await Pagination<ApplicationUser>.ToPagedList(
                query,
                request.PageIndex,
                request.PageSize);

            var responses = new List<ManagerProfileResponse>();
            foreach (var manager in pagedManagers.Items ?? new List<ApplicationUser>())
            {
                var managerProfile = await _unitOfWork.ManagerProfiles.GetByUserIdAsync(manager.Id, cancellationToken);
                responses.Add(MapToManagerProfileResponse(manager, managerProfile));
            }

            return new Pagination<ManagerProfileResponse>(
                responses,
                pagedManagers.TotalCount,
                pagedManagers.CurrentPage,
                pagedManagers.PageSize);
        }

        public async Task<ManagerProfileResponse> CreateManagerAsync(
            CreateManagerAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureUniqueCredentialsAsync(request.Email, request.UserName, request.PhoneNumber, cancellationToken);

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = request.Email.Trim(),
                NormalizedEmail = request.Email.Trim().ToUpperInvariant(),
                UserName = request.UserName.Trim(),
                NormalizedUserName = request.UserName.Trim().ToUpperInvariant(),
                PhoneNumber = request.PhoneNumber.Trim(),
                DisplayName = request.FullName.Trim(),
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            EnsureIdentitySucceeded(createResult);

            var roleResult = await _userManager.AddToRoleAsync(user, Role.Manager.ToString());
            EnsureIdentitySucceeded(roleResult);

            var profile = new ManagerProfile
            {
                ManagerProfileId = Guid.NewGuid(),
                UserId = user.Id,
                AppointedAt = DateTime.UtcNow,
                Notes = request.Notes?.Trim()
            };

            await _unitOfWork.ManagerProfiles.AddAsync(profile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var createdUser = await _unitOfWork.Users.GetByIdWithManagerProfileAsync(user.Id, cancellationToken)
                ?? throw new InvalidOperationException("Created manager not found.");

            return MapToManagerProfileResponse(createdUser, createdUser.ManagerProfile);
        }

        public async Task<ManagerProfileResponse> GetManagerByIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByIdWithManagerProfileAsync(userId, cancellationToken)
                ?? throw new UserNotFoundException(userId.ToString());

            await EnsureUserInRoleAsync(user, Role.Manager);
            return MapToManagerProfileResponse(user, user.ManagerProfile);
        }

        public async Task<ManagerProfileResponse> UpdateManagerAsync(
            Guid userId,
            UpdateManagerAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByIdWithManagerProfileAsync(userId, cancellationToken)
                ?? throw new UserNotFoundException(userId.ToString());

            await EnsureUserInRoleAsync(user, Role.Manager);
            var profile = user.ManagerProfile ?? throw new InvalidOperationException("Manager profile not found.");

            await ApplyBasicUserUpdatesAsync(user, request.Email, request.UserName, request.PhoneNumber, request.FullName, cancellationToken);

            if (request.Notes != null)
                profile.Notes = request.Notes.Trim();

            var updateResult = await _userManager.UpdateAsync(user);
            EnsureIdentitySucceeded(updateResult);
            await _unitOfWork.ManagerProfiles.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedUser = await _unitOfWork.Users.GetByIdWithManagerProfileAsync(user.Id, cancellationToken)
                ?? throw new InvalidOperationException("Updated manager not found.");

            return MapToManagerProfileResponse(updatedUser, updatedUser.ManagerProfile);
        }

        public async Task<ManagerProfileResponse> SoftDeleteManagerAsync(
            Guid userId,
            SoftDeletePrivilegedAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByIdWithManagerProfileAsync(userId, cancellationToken)
                ?? throw new UserNotFoundException(userId.ToString());

            await EnsureUserInRoleAsync(user, Role.Manager);
            await BanUserAsync(userId, new BanUserRequest { Reason = request.Reason }, cancellationToken);

            var updatedUser = await _unitOfWork.Users.GetByIdWithManagerProfileAsync(userId, cancellationToken)
                ?? throw new InvalidOperationException("Manager not found after soft delete.");

            return MapToManagerProfileResponse(updatedUser, updatedUser.ManagerProfile);
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

            Campaign? selectedCampaign = null;
            if (request.CampaignId.HasValue)
            {
                selectedCampaign = await ValidateVolunteerCampaignAsync(request.CampaignId.Value, cancellationToken);
                var existingRegistration = await _unitOfWork.CampaignVolunteerRegistrations
                    .GetByCampaignAndUserAsync(request.CampaignId.Value, userId, cancellationToken);
                if (existingRegistration != null)
                    throw new InvalidOperationException("Bạn đã có đăng ký liên quan đến campaign này rồi.");
            }

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

            if (selectedCampaign != null)
            {
                var registration = new CampaignVolunteerRegistration
                {
                    CampaignVolunteerRegistrationId = Guid.NewGuid(),
                    CampaignId = selectedCampaign.CampaignId,
                    UserId = userId,
                    Status = CampaignVolunteerRegistrationStatus.PendingVolunteerApproval,
                    RegisteredAt = DateTime.UtcNow,
                    User = user,
                    Campaign = selectedCampaign 
                };

                await _unitOfWork.CampaignVolunteerRegistrations.AddAsync(registration, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await BuildVolunteerProfileResponseAsync(volunteerProfile, user, cancellationToken);
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

            var pendingRegistrations = await _unitOfWork.CampaignVolunteerRegistrations.GetByUserAsync(profile.UserId, cancellationToken);
            foreach (var registration in pendingRegistrations.Where(x => x.Status == CampaignVolunteerRegistrationStatus.PendingVolunteerApproval))
            {
                await _campaignService.EnsureVolunteerRegistrationCapacityAsync(registration.CampaignId, cancellationToken);
                registration.Status = CampaignVolunteerRegistrationStatus.Registered;
                await _campaignService.UpdateProgressAsync(registration.CampaignId, CampaignResourceType.People, 1, cancellationToken);
            }


            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var subject = "Hồ sơ tình nguyện viên đã được chấp nhận";
                var body = $@"
                    Xin chào {user.DisplayName},<br/><br/>
                    Hồ sơ đăng ký tình nguyện viên của bạn đã được moderator chấp nhận.<br/>
                    Bạn hiện đã có thể tham gia các chức năng dành cho tình nguyện viên trong hệ thống.<br/><br/>
                    Trân trọng,<br/>
                    ReliefCare";

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }

            return await BuildVolunteerProfileResponseAsync(profile, profile.User, cancellationToken);
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

            var pendingRegistrations = await _unitOfWork.CampaignVolunteerRegistrations.GetByUserAsync(profile.UserId, cancellationToken);
            foreach (var registration in pendingRegistrations.Where(x => x.Status == CampaignVolunteerRegistrationStatus.PendingVolunteerApproval))
            {
                registration.Status = CampaignVolunteerRegistrationStatus.Rejected;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await BuildVolunteerProfileResponseAsync(profile, profile.User, cancellationToken);
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

            return await BuildVolunteerProfileResponseAsync(profile, profile.User, cancellationToken);
        }

        public async Task<VolunteerProfileResponse> ResubmitVolunteerProfileAsync(
            ResubmitVolunteerRequest request,
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId
                         ?? throw new UnauthorizedAccessException("User not authenticated");

            var profile = await _unitOfWork.VolunteerProfiles.GetByUserIdWithSkillsAsync(userId);
            if (profile == null)
                throw new InvalidOperationException("Volunteer profile not found.");

            if (profile.VerificationStatus != VerificationStatus.Rejected)
                throw new InvalidOperationException("Only rejected volunteer profiles can be resubmitted.");

            profile.Descriptions = request.Descriptions;
            profile.YearsOfExperience = request.YearsOfExperience;
            profile.PreferredTeamRole = request.PreferredTeamRole;
            profile.VerificationStatus = VerificationStatus.Pending;
            profile.VerifiedAt = null;
            profile.VerifiedBy = null;
            profile.Reason = null;
            profile.Status = VolunteerStatus.Inactive;

            if (request.CampaignId.HasValue)
            {
                await ValidateVolunteerCampaignAsync(request.CampaignId.Value, cancellationToken);
            }

            var existingRegistrations = await _unitOfWork.CampaignVolunteerRegistrations.GetByUserAsync(userId, cancellationToken);
            foreach (var registration in existingRegistrations.Where(x => x.Status == CampaignVolunteerRegistrationStatus.PendingVolunteerApproval))
            {
                registration.Status = CampaignVolunteerRegistrationStatus.Cancelled;
                registration.CancelledAt = DateTime.UtcNow;
            }

            if (request.CampaignId.HasValue)
            {
                var duplicate = existingRegistrations.FirstOrDefault(x => x.CampaignId == request.CampaignId.Value && x.Status == CampaignVolunteerRegistrationStatus.Registered);
                if (duplicate != null)
                    throw new InvalidOperationException("Bạn đã đăng ký campaign này rồi.");

                var pending = new CampaignVolunteerRegistration
                {
                    CampaignVolunteerRegistrationId = Guid.NewGuid(),
                    CampaignId = request.CampaignId.Value,
                    UserId = userId,
                    Status = CampaignVolunteerRegistrationStatus.PendingVolunteerApproval,
                    RegisteredAt = DateTime.UtcNow,
                    User = profile.User
                };
                await _unitOfWork.CampaignVolunteerRegistrations.AddAsync(pending, cancellationToken);
            }

            profile.VolunteerSkills.Clear();
            foreach (var skillId in request.SkillIds.Distinct())
            {
                profile.VolunteerSkills.Add(new VolunteerSkill
                {
                    VolunteerProfileId = profile.VolunteerProfileId,
                    SkillId = skillId
                });
            }

            profile.Certificates.Clear();
            foreach (var certificate in request.Certificates)
            {
                profile.Certificates.Add(new VolunteerCertificate
                {
                    VolunteerProfileId = profile.VolunteerProfileId,
                    Name = certificate.Name,
                    IssuedBy = certificate.IssuedBy,
                    IssuedDate = certificate.IssuedDate,
                    ExpiryDate = certificate.ExpiryDate,
                    FileUrl = certificate.FileUrl
                });
            }

            await _unitOfWork.VolunteerProfiles.UpdateAsync(profile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await BuildVolunteerProfileResponseAsync(profile, profile.User, cancellationToken);
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

            var users = pagedUsers.Items!
                .Where(u => u.VolunteerProfile != null)
                .ToList();

            var items = new List<VolunteerProfileResponse>(users.Count);
            foreach (var user in users)
            {
                var registrations = await _unitOfWork.CampaignVolunteerRegistrations.GetByUserAsync(user.Id, cancellationToken);
                var selectedRegistration = SelectPreferredRegistration(registrations);
                items.Add(MapToResponse(user.VolunteerProfile!, user, selectedRegistration));
            }

            return new Pagination<VolunteerProfileResponse>(
                items,
                pagedUsers.TotalCount,
                pagedUsers.CurrentPage,
                pagedUsers.PageSize);
        }

        public async Task<Pagination<VolunteerProfileResponse>> GetUnassignedVolunteersAsync(
            SearchVolunteerProfilesRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.Users.GetQueryableWithVolunteerProfile()
                .Where(u => u.VolunteerProfile != null && u.VolunteerProfile.Status == VolunteerStatus.Active && !u.TeamMembers.Any())
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

            var users = pagedUsers.Items!
                .Where(u => u.VolunteerProfile != null)
                .ToList();

            var items = new List<VolunteerProfileResponse>(users.Count);
            foreach (var user in users)
            {
                var registrations = await _unitOfWork.CampaignVolunteerRegistrations.GetByUserAsync(user.Id, cancellationToken);
                var selectedRegistration = SelectPreferredRegistration(registrations);
                items.Add(MapToResponse(user.VolunteerProfile!, user, selectedRegistration));
            }

            return new Pagination<VolunteerProfileResponse>(
                items,
                pagedUsers.TotalCount,
                pagedUsers.CurrentPage,
                pagedUsers.PageSize);
        }

        public async Task<List<VolunteerProfileResponse>> GetAllUnassignedVolunteersListAsync(
            CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.Users.GetQueryableWithVolunteerProfile()
                .Where(u => u.VolunteerProfile != null && u.VolunteerProfile.Status == VolunteerStatus.Active && !u.TeamMembers.Any())
                .OrderBy(u => u.DisplayName ?? u.Email ?? string.Empty);

            var users = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(query, cancellationToken);

            var results = new List<VolunteerProfileResponse>(users.Count);
            foreach (var user in users)
            {
                var registrations = await _unitOfWork.CampaignVolunteerRegistrations.GetByUserAsync(user.Id, cancellationToken);
                var selectedRegistration = SelectPreferredRegistration(registrations);
                results.Add(MapToResponse(user.VolunteerProfile!, user, selectedRegistration));
            }

            return results;
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
           ApplicationUser? user,
           CampaignVolunteerRegistration? registration = null)
        {
            return new VolunteerProfileResponse
            {
                UserId = user.Id,
                VolunteerProfileId = profile.VolunteerProfileId,
                FullName = user?.DisplayName,
                Email = user?.Email,
                PhoneNumber = user?.PhoneNumber,
                Descriptions = profile.Descriptions,
                VerificationStatus = profile.VerificationStatus,
                Reason = profile.Reason,
                CampaignId = registration?.CampaignId,
                CampaignName = registration?.Campaign?.Name,
                CampaignRegistrationStatus = registration?.Status,
                YearsOfExperience = profile.YearsOfExperience,
                PreferredTeamRole = profile.PreferredTeamRole,
                Skills = profile.VolunteerSkills
                    .Select(vs => vs.SkillId)
                    .ToList(),
                Certificates = profile.Certificates.Select(c => new VolunteerCertificateResponse
                {
                    Name = c.Name,
                    IssuedBy = c.IssuedBy,
                    IssuedDate = c.IssuedDate,
                    ExpiryDate = c.ExpiryDate,
                    FileUrl = c.FileUrl
                }).ToList()
            };
        }

        private async Task<VolunteerProfileResponse> BuildVolunteerProfileResponseAsync(
            VolunteerProfile profile,
            ApplicationUser? user,
            CancellationToken cancellationToken)
        {
            var registrations = await _unitOfWork.CampaignVolunteerRegistrations.GetByUserAsync(profile.UserId, cancellationToken);
            var selectedRegistration = SelectPreferredRegistration(registrations);

            return MapToResponse(profile, user, selectedRegistration);
        }

        private static CampaignVolunteerRegistration? SelectPreferredRegistration(IEnumerable<CampaignVolunteerRegistration> registrations)
        {
            var ordered = registrations.OrderByDescending(r => r.RegisteredAt);

            return ordered.FirstOrDefault(r => r.Status == CampaignVolunteerRegistrationStatus.PendingVolunteerApproval)
                ?? ordered.FirstOrDefault(r => r.Status == CampaignVolunteerRegistrationStatus.Registered)
                ?? ordered.FirstOrDefault();
        }

        private async Task<Campaign> ValidateVolunteerCampaignAsync(Guid campaignId, CancellationToken cancellationToken)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithGoalsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            if (campaign.Type != CampaignType.Fundraising)
                throw new InvalidOperationException("Chỉ fundraising campaign mới cho phép đăng ký volunteer.");

            if (campaign.Status != CampaignStatus.Active)
                throw new InvalidOperationException("Chỉ campaign đang Active mới cho phép đăng ký volunteer.");

            var peopleGoal = campaign.ResourceGoals.FirstOrDefault(g => g.ResourceType == CampaignResourceType.People);
            if (peopleGoal is null)
                throw new InvalidOperationException("Campaign này không có mục tiêu People để đăng ký volunteer.");

            return campaign;
        }

        private static ModeratorProfileResponse MapToModeratorProfileResponse(
            ApplicationUser user,
            ModeratorProfile? moderatorProfile)
        {
            var now = DateTimeOffset.UtcNow;
            return new ModeratorProfileResponse
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                PictureUrl = user.PictureUrl,
                IsBanned = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > now,
                LockoutEnd = user.LockoutEnd,
                BanReason = user.BanReason,
                ModeratorStatus = moderatorProfile?.Status,
                IsStationHead = moderatorProfile?.IsStationHead ?? false,
                IsManagingStation = moderatorProfile?.ReliefStationId != null,
                ReliefStationId = moderatorProfile?.ReliefStationId,
                ReliefStationName = moderatorProfile?.ReliefStation?.Name,
                AppointedAt = moderatorProfile?.AppointedAt ?? default,
                Notes = moderatorProfile?.Notes,
                StatusReason = moderatorProfile?.StatusReason
            };
        }

        private static ManagerProfileResponse MapToManagerProfileResponse(
            ApplicationUser user,
            ManagerProfile? managerProfile)
        {
            var now = DateTimeOffset.UtcNow;
            return new ManagerProfileResponse
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                PictureUrl = user.PictureUrl,
                IsBanned = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > now,
                LockoutEnd = user.LockoutEnd,
                BanReason = user.BanReason,
                AppointedAt = managerProfile?.AppointedAt ?? default,
                Notes = managerProfile?.Notes
            };
        }

        private async Task EnsureUniqueCredentialsAsync(
            string email,
            string userName,
            string phoneNumber,
            CancellationToken cancellationToken)
        {
            if (await _userManager.FindByEmailAsync(email.Trim()) != null)
                throw new InvalidOperationException("Email already exists.");

            if (await _userManager.FindByNameAsync(userName.Trim()) != null)
                throw new InvalidOperationException("Username already exists.");

            var existingPhone = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber.Trim(), cancellationToken);
            if (existingPhone != null)
                throw new InvalidOperationException("Phone number already exists.");
        }

        private async Task ApplyBasicUserUpdatesAsync(
            ApplicationUser user,
            string? email,
            string? userName,
            string? phoneNumber,
            string? fullName,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(email) && !string.Equals(user.Email, email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userManager.FindByEmailAsync(email.Trim());
                if (existing != null && existing.Id != user.Id)
                    throw new InvalidOperationException("Email already exists.");
                user.Email = email.Trim();
                user.NormalizedEmail = email.Trim().ToUpperInvariant();
            }

            if (!string.IsNullOrWhiteSpace(userName) && !string.Equals(user.UserName, userName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userManager.FindByNameAsync(userName.Trim());
                if (existing != null && existing.Id != user.Id)
                    throw new InvalidOperationException("Username already exists.");
                user.UserName = userName.Trim();
                user.NormalizedUserName = userName.Trim().ToUpperInvariant();
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber) && !string.Equals(user.PhoneNumber, phoneNumber.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber.Trim(), cancellationToken);
                if (existing != null && existing.Id != user.Id)
                    throw new InvalidOperationException("Phone number already exists.");
                user.PhoneNumber = phoneNumber.Trim();
            }

            if (!string.IsNullOrWhiteSpace(fullName))
                user.DisplayName = fullName.Trim();
        }

        private async Task EnsureUserInRoleAsync(ApplicationUser user, Role role)
        {
            if (!await _userManager.IsInRoleAsync(user, role.ToString()))
                throw new InvalidOperationException($"User is not in role {role}.");
        }

        private async Task EnsureStationHeadAvailableAsync(
            Guid reliefStationId,
            Guid? currentModeratorUserId,
            CancellationToken cancellationToken)
        {
            var currentHead = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(reliefStationId, cancellationToken);
            if (currentHead != null && currentHead.UserId != currentModeratorUserId)
            {
                throw new InvalidOperationException("Relief station already has another station head moderator.");
            }
        }

        private static void EnsureIdentitySucceeded(IdentityResult result)
        {
            if (result.Succeeded)
                return;

            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            throw new ReliefManagementSystem.Application.Common.Exceptions.ValidationException(errors);
        }
    }
}
