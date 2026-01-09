using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.Team.Response;
using ReliefManagementSystem.Application.Features.TeamJoinRequest.Request;
using ReliefManagementSystem.Application.Features.TeamJoinRequest.Response;
using ReliefManagementSystem.Application.Services;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Services
{
    public class TeamJoinRequestService : ITeamJoinRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITeamJoinRequestRepository _requestRepository;
        private readonly ITeamRepository _teamRepository;
        private readonly ITeamMemberRepository _teamMemberRepository;
        private readonly IVolunteerProfileRepository _volunteerProfileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeamJoinRequestService(
            ApplicationDbContext context,
            ITeamJoinRequestRepository requestRepository,
            ITeamRepository teamRepository,
            ITeamMemberRepository teamMemberRepository,
            IVolunteerProfileRepository volunteerProfileRepository,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _requestRepository = requestRepository;
            _teamRepository = teamRepository;
            _teamMemberRepository = teamMemberRepository;
            _volunteerProfileRepository = volunteerProfileRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<TeamJoinRequestResponse> CreateRequestAsync(
            CreateTeamJoinRequest request,
            Guid volunteerId,
            CancellationToken cancellationToken)
        {
            // 1. Validate volunteer and get profile with skills
            var volunteer = await _context.Users
                .Include(u => u.VolunteerProfile)
                    .ThenInclude(vp => vp.VolunteerSkills)
                        .ThenInclude(vs => vs.Skill)
                .FirstOrDefaultAsync(u => u.Id == volunteerId, cancellationToken);

            if (volunteer == null)
                throw new Exception("Không tìm thấy tình nguyện viên");

            var isVolunteer = await _userManager.IsInRoleAsync(volunteer, "Volunteer");
            if (!isVolunteer)
                throw new Exception("Chỉ có tình nguyện viên mới được yêu cầu tham gia");

            if (volunteer.VolunteerProfile?.VerificationStatus != VerificationStatus.Approved)
                throw new Exception("Hồ sơ tình nguyên phải được duyệt");

            // 2. Validate team exists and active
            var team = await _context.Teams
                .Include(t => t.Moderator)
                .FirstOrDefaultAsync(t => t.TeamId == request.TeamId, cancellationToken);

            if (team == null)
                throw new Exception("Không tìm thấy đội");

            if (team.Status != TeamStatus.Active)
                throw new Exception("Đội chưa hoạt động ");

            // 3. Check not already a member
            var isMember = await _teamMemberRepository.IsMemberAsync(request.TeamId, volunteerId);
            if (isMember)
                throw new Exception("Bạn đã là thành viên đội này rồi");

            // 4. Check no existing pending request
            var existingRequest = await _requestRepository.GetExistingPendingRequestAsync(request.TeamId, volunteerId);
            if (existingRequest != null)
                throw new Exception("Bạn đã gửi yêu cầu gia nhập cho đội này rồi");

            // 5. If requesting Leader role, check team has no leader
            if (request.RequestedRole == TeamRole.Leader && team.LeaderId.HasValue)
                throw new Exception("Đội này đã có trưởng nhóm");

            // 6. Create request
            var joinRequest = new TeamJoinRequest
            {
                Id = Guid.NewGuid(),
                TeamId = request.TeamId,
                VolunteerId = volunteerId,
                RequestedRole = request.RequestedRole,
                Status = TeamJoinRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _requestRepository.AddAsync(joinRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 7. Return response with volunteer skills
            return new TeamJoinRequestResponse
            {
                Id = joinRequest.Id,
                TeamId = team.TeamId,
                TeamName = team.Name,
                ModeratorId = team.ModeratorId,
                ModeratorName = team.Moderator.DisplayName ?? team.Moderator.UserName ?? "Unknown",
                VolunteerId = volunteerId,
                VolunteerName = volunteer.DisplayName ?? volunteer.UserName ?? "Unknown",
                VolunteerEmail = volunteer.Email ?? "",
                VolunteerSkills = volunteer.VolunteerProfile.VolunteerSkills.Select(vs => new SkillInfo
                {
                    SkillId = vs.SkillId,
                    Code = vs.Skill.Code,
                    Name = vs.Skill.Name,
                    Description = vs.Skill.Description
                }).ToList(),
                RequestedRole = joinRequest.RequestedRole,
                Status = joinRequest.Status,
                CreatedAt = joinRequest.CreatedAt
            };
        }

        public async Task<bool> CancelRequestAsync(
            Guid requestId,
            Guid volunteerId,
            CancellationToken cancellationToken)
        {
            var request = await _requestRepository.GetByIdAsync(requestId);

            if (request == null)
                throw new Exception("Không tìm thấy yêu cầu");

            if (request.VolunteerId != volunteerId)
                throw new Exception("Chỉ có người tạo yêu cầu mới được huỷ");

            if (request.Status != TeamJoinRequestStatus.Pending)
                throw new Exception("Chỉ có thể huỷ yêu cầu đang được duyệt");

            request.Status = TeamJoinRequestStatus.Cancelled;
            await _requestRepository.UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<List<TeamJoinRequestResponse>> GetMyRequestsAsync(
            Guid volunteerId,
            CancellationToken cancellationToken)
        {
            var requests = await _requestRepository.GetByVolunteerIdWithDetailsAsync(volunteerId);
            return requests.Select(MapToResponse).ToList();
        }

        public async Task<TeamJoinRequestResponse> ReviewRequestAsync(
            Guid requestId,
            ReviewTeamJoinRequest request,
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            // 1. Load request with all details
            var joinRequest = await _requestRepository.GetByIdWithDetailsAsync(requestId);

            if (joinRequest == null)
                throw new Exception("Không tìm thấy yêu cầu");

            // 2. Validate moderator
            if (joinRequest.Team.ModeratorId != moderatorId)
                throw new Exception("Chỉ có điều phối team mới có thể xem xét");

            if (joinRequest.Status != TeamJoinRequestStatus.Pending)
                throw new Exception("Chỉ có thể xem xét các yêu cầu đang được duyệt");

            // 3. Process approval
            if (request.IsApproved)
            {
                // Validate leader if requesting Leader role
                if (joinRequest.RequestedRole == TeamRole.Leader && joinRequest.Team.LeaderId.HasValue)
                    throw new Exception("Đội này đã có trưởng nhóm");

                // Create TeamMember
                var teamMember = new TeamMember
                {
                    TeamId = joinRequest.TeamId,
                    UserId = joinRequest.VolunteerId,
                    RoleTeam = joinRequest.RequestedRole,
                    JoinedAt = DateTime.UtcNow
                };

                await _teamMemberRepository.AddAsync(teamMember);

                // If Leader role, update Team.LeaderId
                if (joinRequest.RequestedRole == TeamRole.Leader)
                {
                    joinRequest.Team.LeaderId = joinRequest.VolunteerId;
                    joinRequest.Team.UpdatedAt = DateTime.UtcNow;
                    await _teamRepository.UpdateAsync(joinRequest.Team);
                }

                joinRequest.Status = TeamJoinRequestStatus.Approved;
            }
            else
            {
                joinRequest.Status = TeamJoinRequestStatus.Rejected;
            }

            // 4. Update review info
            joinRequest.ReviewedBy = moderatorId;
            joinRequest.ReviewedAt = DateTime.UtcNow;
            joinRequest.ReviewNote = request.ReviewNote;

            await _requestRepository.UpdateAsync(joinRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(joinRequest);
        }

        public async Task<List<TeamJoinRequestResponse>> GetPendingRequestsForMyTeamsAsync(
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            var requests = await _requestRepository.GetPendingRequestsByModeratorWithDetailsAsync(moderatorId);
            return requests.Select(MapToResponse).ToList();
        }

        public async Task<List<TeamJoinRequestResponse>> GetRequestsByTeamAsync(
            Guid teamId,
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            // Validate moderator owns team
            var isModerator = await _teamRepository.IsModeratorOfTeamAsync(teamId, moderatorId);
            if (!isModerator)
                throw new Exception("Chỉ có người điều phối của team mới có thể xem yêu cầu");

            var requests = await _requestRepository.GetByTeamIdWithDetailsAsync(teamId);
            return requests.Select(MapToResponse).ToList();
        }

        public async Task<TeamJoinRequestResponse> GetRequestByIdAsync(
            Guid requestId,
            CancellationToken cancellationToken)
        {
            var request = await _requestRepository.GetByIdWithDetailsAsync(requestId);

            if (request == null)
                throw new Exception("Không thấy yêu cầu");

            return MapToResponse(request);
        }

        // Helper method
        private TeamJoinRequestResponse MapToResponse(TeamJoinRequest request)
        {
            return new TeamJoinRequestResponse
            {
                Id = request.Id,
                TeamId = request.TeamId,
                TeamName = request.Team.Name,
                ModeratorId = request.Team.ModeratorId,
                ModeratorName = request.Team.Moderator.DisplayName ?? request.Team.Moderator.UserName ?? "Unknown",
                VolunteerId = request.VolunteerId,
                VolunteerName = request.Volunteer.DisplayName ?? request.Volunteer.UserName ?? "Unknown",
                VolunteerEmail = request.Volunteer.Email ?? "",
                VolunteerSkills = request.Volunteer.VolunteerProfile?.VolunteerSkills.Select(vs => new SkillInfo
                {
                    SkillId = vs.SkillId,
                    Code = vs.Skill.Code,
                    Name = vs.Skill.Name,
                    Description = vs.Skill.Description
                }).ToList() ?? new List<SkillInfo>(),
                RequestedRole = request.RequestedRole,
                Status = request.Status,
                ReviewedBy = request.ReviewedBy,
                ReviewerName = request.Reviewer?.DisplayName ?? request.Reviewer?.UserName,
                ReviewNote = request.ReviewNote,
                CreatedAt = request.CreatedAt,
                ReviewedAt = request.ReviewedAt
            };
        }
    }
}
