
using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Team.DTOs.Request;
using ReliefManagementSystem.Application.Features.Team.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Application.Common.Exceptions.Team;
using ReliefManagementSystem.Application.Common.Exceptions.TeamMember;
using ReliefManagementSystem.Application.Common.Exceptions.Volunteer;
using ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions;

namespace ReliefManagementSystem.Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IRescueRequestService _rescueRequestService;

        public TeamService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IRescueRequestService rescueRequestService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _rescueRequestService = rescueRequestService;
        }

        //Create a team
        public async Task<TeamResponse> CreateTeamAsync(
            CreateTeamRequest request,
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            var normalizedName = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new TeamValidationException("Tên đội là bắt buộc");

            var normalizedDescription = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();

            var normalizedPhone = string.IsNullOrWhiteSpace(request.ContactPhone)
                ? null
                : request.ContactPhone.Trim();

            ValidateTeamType(request.TeamType);

            var team = new Team
            { 
                Name = normalizedName,
                Description = normalizedDescription,
                ContactPhone = normalizedPhone,
                CreateBy = moderatorId,
                LeaderId = null, 
                TeamType = request.TeamType,
                Status = TeamStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Teams.AddAsync(team);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Nếu moderator thuộc một trạm thì tự động gắn team vào trạm đó
            var moderatorProfile = await _unitOfWork.ModeratorProfiles
                .GetByUserIdAsync(moderatorId, cancellationToken);

            if (moderatorProfile?.ReliefStationId != null)
            {
                var existingAssignment = await _unitOfWork.ReliefStationTeams
                    .GetByStationAndTeamAsync(moderatorProfile.ReliefStationId.Value, team.TeamId, cancellationToken);

                if (existingAssignment == null)
                {
                    var assignment = new ReliefStationTeam
                    {
                        ReliefStationTeamId = Guid.NewGuid(),
                        ReliefStationId = moderatorProfile.ReliefStationId.Value,
                        TeamId = team.TeamId,
                        Status = ReliefTeamAssignmentStatus.Approved,
                        Description = "Auto-assigned when moderator created team",
                        JoinedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.ReliefStationTeams.AddAsync(assignment);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
            }

            return await MapToTeamResponse(team, cancellationToken);
        }

        //Get team with team ID
        public async Task<TeamDetailResponse> GetTeamByIdAsync(
            Guid teamId,
            CancellationToken cancellationToken)
        {
            var team = await _unitOfWork.Teams.GetByIdWithDetailsAsync(teamId);

            if (team == null)
                throw new TeamNotFoundException();

            return MapToTeamDetailResponse(team);
        }

        //Update team
        public async Task<TeamResponse> UpdateTeamAsync(
            Guid teamId,
            UpdateTeamRequest request,
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            var normalizedName = request.Name?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new TeamValidationException("Tên đội là bắt buộc");

            var normalizedDescription = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();

            var normalizedPhone = string.IsNullOrWhiteSpace(request.ContactPhone)
                ? null
                : request.ContactPhone.Trim();

            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);

            if (team == null)
                throw new TeamNotFoundException();

            //if (team.CreateBy != moderatorId)
            //    throw new UnauthorizedTeamActionException("chỉnh sửa");

            if (request.LeaderId != team.LeaderId)
            {
                if (request.LeaderId.HasValue)
                {
                    var newLeader = await _unitOfWork.Users.GetByIdWithVolunteerProfileAsync(
                        request.LeaderId.Value, cancellationToken);

                    if (newLeader == null)
                        throw new VolunteerNotFoundException();

                    if (newLeader.VolunteerProfile?.VerificationStatus != VerificationStatus.Approved)
                        throw new VolunteerNotVerifiedException("Trưởng nhóm phải là tình nguyện viên đã được xác minh");

                    var isNewLeaderMember = await _unitOfWork.TeamMembers.IsMemberAsync(teamId, request.LeaderId.Value);

                    if (!isNewLeaderMember)
                    {
                        var newTeamMember = new TeamMember
                        {
                            TeamId = teamId,
                            UserId = request.LeaderId.Value,
                            RoleTeam = TeamRole.Leader,
                            JoinedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.TeamMembers.AddAsync(newTeamMember);
                    }
                    else
                    {
                        var existingMember = await _unitOfWork.TeamMembers.GetByTeamAndUserAsync(teamId, request.LeaderId.Value);
                        if (existingMember != null)
                        {
                            existingMember.RoleTeam = TeamRole.Leader;
                            await _unitOfWork.TeamMembers.UpdateAsync(existingMember);
                        }
                    }
                }

                if (team.LeaderId.HasValue)
                {
                    var oldLeader = await _unitOfWork.TeamMembers.GetByTeamAndUserAsync(teamId, team.LeaderId.Value);
                    if (oldLeader != null)
                    {
                        oldLeader.RoleTeam = TeamRole.Member;
                        await _unitOfWork.TeamMembers.UpdateAsync(oldLeader);
                    }
                }

                team.LeaderId = request.LeaderId;
            }

            team.Name = normalizedName;
            team.Description = normalizedDescription;
            team.ContactPhone = normalizedPhone;
            team.Status = request.Status;

            if (request.TeamType.HasValue)
            {
                ValidateTeamType(request.TeamType.Value);
                team.TeamType = request.TeamType.Value;
            }

            team.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Teams.UpdateAsync(team);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapToTeamResponse(team, cancellationToken);
        }

        //Delete a team
        public async Task<bool> DeleteTeamAsync(
            Guid teamId,
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);

            if (team == null)
                throw new TeamNotFoundException();

            if (team.CreateBy != moderatorId)
                throw new UnauthorizedTeamActionException("xoá");

            await _unitOfWork.Teams.DeleteAsync(team);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
        
        //Get team list
        public async Task<Pagination<TeamResponse>> GetAllTeamsAsync(SearchTeamRequest request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Teams.GetQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                query = query.Where(t =>
                    (t.Name ?? string.Empty).Contains(keyword) ||
                    (t.Description ?? string.Empty).Contains(keyword) ||
                    (t.ContactPhone ?? string.Empty).Contains(keyword));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(t => t.Status == request.Status.Value);
            }

            if (request.TeamType.HasValue)
            {
                query = query.Where(t => t.TeamType == request.TeamType.Value);
            }

            if (request.ModeratorId.HasValue)
            {
                query = query.Where(t => t.CreateBy == request.ModeratorId.Value);
            }

            query = query.OrderByDescending(t => t.CreatedAt);

            var pagedTeams = await Pagination<Team>.ToPagedList(query, request.PageIndex, request.PageSize);

            var responseItems = new List<TeamResponse>();
            foreach (var team in pagedTeams.Items!)
            {
                responseItems.Add(await MapToTeamResponse(team, cancellationToken));
            }

            return new Pagination<TeamResponse>(responseItems, pagedTeams.TotalCount, pagedTeams.CurrentPage, pagedTeams.PageSize);
        }

        //Search team
        public async Task<Pagination<TeamResponse>> SearchTeamsAsync(
            SearchTeamRequest request,
            CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Teams.GetQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                query = query.Where(t =>
                    (t.Name ?? string.Empty).Contains(keyword) ||
                    (t.Description ?? string.Empty).Contains(keyword) ||
                    (t.ContactPhone ?? string.Empty).Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(t => t.Name.Contains(request.Name));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(t => t.Status == request.Status.Value);
            }

            if (request.TeamType.HasValue)
            {
                query = query.Where(t => t.TeamType == request.TeamType.Value);
            }

            if (request.ModeratorId.HasValue)
            {
                query = query.Where(t => t.CreateBy == request.ModeratorId.Value);
            }

            query = query.OrderByDescending(t => t.CreatedAt);

            var pagedTeams = await Pagination<Team>.ToPagedList(query, request.PageIndex, request.PageSize);

            var responseItems = new List<TeamResponse>();
            foreach (var team in pagedTeams.Items)
            {
                responseItems.Add(await MapToTeamResponse(team, cancellationToken));
            }

            return new Pagination<TeamResponse>(
                responseItems,
                pagedTeams.TotalCount,
                pagedTeams.CurrentPage,
                pagedTeams.PageSize
            );
        }

        public async Task<Pagination<TeamResponse>> GetTeamsInStationAsync(
            GetTeamsInStationRequest request,
            CancellationToken cancellationToken)
        {
            var station = await _unitOfWork.ReliefStations.GetByIdAsync(request.ReliefStationId);
            if (station == null)
                throw new ReliefStationNotFoundException(request.ReliefStationId);

            var query = _unitOfWork.ReliefStationTeams.GetQueryableWithTeamDetails()
                .Where(x => x.ReliefStationId == request.ReliefStationId)
                .Where(x => x.Status == ReliefTeamAssignmentStatus.Approved)
                .Select(x => x.Team)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                query = query.Where(t =>
                    t.Name.Contains(keyword) ||
                    ((t.Leader != null) &&
                     (((t.Leader.DisplayName ?? string.Empty).Contains(keyword)) ||
                      ((t.Leader.UserName ?? string.Empty).Contains(keyword)))));
            }

            query = query.OrderByDescending(t => t.CreatedAt);

            var pagedTeams = await Pagination<Team>.ToPagedList(query, request.PageIndex, request.PageSize);

            var responseItems = new List<TeamResponse>();
            foreach (var team in pagedTeams.Items!)
            {
                responseItems.Add(await MapToTeamResponse(team, cancellationToken));
            }

            return new Pagination<TeamResponse>(
                responseItems,
                pagedTeams.TotalCount,
                pagedTeams.CurrentPage,
                pagedTeams.PageSize);
        }

        //Get team that moderator manage
        public async Task<List<TeamResponse>> GetMyTeamsAsync(
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            var teams = await _unitOfWork.Teams.GetByModeratorIdAsync(moderatorId);

            var responses = new List<TeamResponse>();
            foreach (var team in teams)
            {
                responses.Add(await MapToTeamResponse(team, cancellationToken));
            }

            return responses;
        }

        //Get team member infomation
        public async Task<List<TeamMemberInfo>> GetTeamMembersAsync(
            Guid teamId,
            CancellationToken cancellationToken)
        {
            var members = await _unitOfWork.TeamMembers.GetByTeamIdWithSkillsAsync(teamId);

            return members.Select(m => new TeamMemberInfo
            {
                UserId = m.UserId,
                VolunteerProfileId = m.User.VolunteerProfile?.VolunteerProfileId,
                DisplayName = m.User.DisplayName ?? m.User.UserName ?? m.User.Email ?? "Unknown",
                Email = m.User.Email ?? "",
                Role = m.RoleTeam,
                Skills = m.User.VolunteerProfile?.VolunteerSkills.Select(vs => new SkillInfo
                {
                    SkillId = vs.SkillId,
                    Code = vs.Skill.Code,
                    Name = vs.Skill.Name,
                    Description = vs.Skill.Description
                }).ToList() ?? new List<SkillInfo>(),
                JoinedAt = m.JoinedAt
            }).ToList();
        }

        //Remove team member 
        public async Task<bool> RemoveMemberAsync(
            Guid teamId,
            Guid userId,
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            if (team == null)
                throw new TeamNotFoundException();

            //if (team.CreateBy != moderatorId)
            //    throw new UnauthorizedTeamActionException("xoá thành viên");

            if (team.LeaderId == userId)
                throw new CannotRemoveLeaderException();

            var member = await _unitOfWork.TeamMembers.GetByTeamAndUserAsync(teamId, userId);
            if (member == null)
                throw new TeamMemberNotFoundException();

            await _unitOfWork.TeamMembers.DeleteAsync(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        //Get volunteer team have join in
        public async Task<TeamDetailResponse> GetVolunteerTeamAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            // SỬA: GetTeamByUserIdAsync trả về TeamMember? (không phải List)
            var teamMember = await _unitOfWork.TeamMembers.GetTeamByUserIdAsync(userId);
            
            if (teamMember == null)
                throw new NotTeamMemberException();

            var team = await _unitOfWork.Teams.GetByIdWithDetailsAsync(teamMember.TeamId);
            
            if (team == null)
                throw new TeamNotFoundException();

            return MapToTeamDetailResponse(team);
        }

        public async Task<List<AssignedCampaignInfo>> GetAssignedCampaignsAsync(
            Guid teamId,
            CancellationToken cancellationToken)
        {
            var team = await _unitOfWork.Teams.GetByIdWithDetailsAsync(teamId);

            if (team == null)
                throw new TeamNotFoundException();

            return team.CampaignTeams
                .Where(ct => !ct.IsDelete)
                .OrderByDescending(ct => ct.AssignedAt)
                .Select(ct => new AssignedCampaignInfo
                {
                    CampaignId = ct.CampaignId,
                    CampaignName = ct.Campaign.Name,
                    CampaignType = (int)ct.Campaign.Type,
                    CampaignTeamId = ct.CampaignTeamId,
                    Status = (int)ct.Status,
                    Role = (int)ct.Role
                })
                .ToList();
        }

        //Moderator add volunteer into team
        public async Task<TeamMemberResponse> AddMemberDirectlyAsync(
            Guid teamId,
            AddMemberRequest request,
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            
            if (team == null)
                throw new TeamNotFoundException();


            var volunteer = await _unitOfWork.Users.GetByIdWithVolunteerProfileAndSkillsAsync(
                request.VolunteerId, cancellationToken);
            
            if (volunteer == null)
                throw new VolunteerNotFoundException();

            if (volunteer.VolunteerProfile?.VerificationStatus != VerificationStatus.Approved)
                throw new VolunteerNotVerifiedException();

            var isMember = await _unitOfWork.TeamMembers.IsMemberAsync(teamId, request.VolunteerId);
            if (isMember)
                throw new DuplicateTeamMemberException();

            var teamMember = new TeamMember
            {
                TeamId = teamId,
                UserId = request.VolunteerId,
                RoleTeam = TeamRole.Member,
                JoinedAt = DateTime.UtcNow
            };

            await _unitOfWork.TeamMembers.AddAsync(teamMember);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TeamMemberResponse
            {
                UserId = volunteer.Id,
                DisplayName = volunteer.DisplayName ?? volunteer.UserName ?? "Unknown",
                Email = volunteer.Email ?? "",
                RoleTeam = teamMember.RoleTeam,
                JoinedAt = teamMember.JoinedAt,
                Skills = volunteer.VolunteerProfile?.VolunteerSkills?.Select(vs => new SkillInfo
                {
                    SkillId = vs.SkillId,
                    Code = vs.Skill.Code,
                    Name = vs.Skill.Name,
                    Description = vs.Skill.Description
                }).ToList()
            };
        }

        public async Task<AddMembersResponse> AddMembersDirectlyAsync(
            Guid teamId,
            AddMembersRequest request,
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            var result = new AddMembersResponse
            {
                TeamId = teamId,
                TotalRequested = request.VolunteerIds?.Count ?? 0
            };

            if (request.VolunteerIds == null || request.VolunteerIds.Count == 0)
            {
                result.FailedCount = 0;
                result.SuccessCount = 0;
                return result;
            }

            var distinctVolunteerIds = request.VolunteerIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            if (team == null)
                throw new TeamNotFoundException();


            foreach (var volunteerId in distinctVolunteerIds)
            {
                try
                {
                    var singleResult = await AddMemberDirectlyAsync(
                        teamId,
                        new AddMemberRequest { VolunteerId = volunteerId },
                        moderatorId,
                        cancellationToken);

                    result.AddedMembers.Add(singleResult);
                }
                catch (Exception ex)
                {
                    result.FailedMembers.Add(new AddMemberFailureItem
                    {
                        VolunteerId = volunteerId,
                        Reason = ex.Message
                    });
                }
            }

            result.SuccessCount = result.AddedMembers.Count;
            result.FailedCount = result.FailedMembers.Count;

            return result;
        }

        //Update role for member role to leader role
        public async Task<TeamMemberResponse> PromoteMemberToLeaderAsync(
            Guid teamId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId
                ?? throw new UnauthorizedTeamActionException("cập nhật role");

            var team = await _unitOfWork.Teams.GetByIdWithDetailsAsync(teamId);
            
            if (team == null)
                throw new TeamNotFoundException();

            if (team.CreateBy != currentUserId)
                throw new UnauthorizedTeamActionException("cập nhật role");

            var teamMember = await _unitOfWork.TeamMembers.GetByTeamAndUserWithSkillsAsync(
                teamId, userId);
            
            if (teamMember == null)
                throw new TeamMemberNotFoundException("Người dùng không phải là thành viên team");

            if (teamMember.RoleTeam == TeamRole.Leader)
                throw new TeamMemberAlreadyLeaderException();

            if (team.LeaderId.HasValue && team.LeaderId.Value != userId)
            {
                var currentLeader = await _unitOfWork.TeamMembers.GetByTeamAndUserAsync(
                    teamId, team.LeaderId.Value);
                
                if (currentLeader != null)
                {
                    currentLeader.RoleTeam = TeamRole.Member;
                    await _unitOfWork.TeamMembers.UpdateAsync(currentLeader);
                }
            }

            teamMember.RoleTeam = TeamRole.Leader;
            await _unitOfWork.TeamMembers.UpdateAsync(teamMember);

            team.LeaderId = userId;
            team.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Teams.UpdateAsync(team);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TeamMemberResponse
            {
                UserId = teamMember.UserId,
                DisplayName = teamMember.User.DisplayName ?? teamMember.User.UserName ?? "Unknown",
                Email = teamMember.User.Email ?? "",
                RoleTeam = teamMember.RoleTeam,
                JoinedAt = teamMember.JoinedAt,
                Skills = teamMember.User.VolunteerProfile?.VolunteerSkills?.Select(vs => new SkillInfo
                {
                    SkillId = vs.SkillId,
                    Code = vs.Skill.Code,
                    Name = vs.Skill.Name,
                    Description = vs.Skill.Description
                }).ToList()
            };
        }

        public async Task<TeamTrackingPointResponse> TrackTeamHeartbeatAsync(
            Guid teamId,
            TeamTrackingHeartbeatRequest request,
            CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.UserId
                ?? throw new UnauthorizedTeamActionException("gửi vị trí");

            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            if (team == null)
                throw new TeamNotFoundException();

            var isMember = await _unitOfWork.TeamMembers.IsMemberAsync(teamId, currentUserId);
            if (!isMember)
                throw new UnauthorizedTeamActionException("gửi vị trí");

            var capturedAtUtc = request.CapturedAtUtc == default || request.CapturedAtUtc == DateTime.MinValue
                ? DateTime.UtcNow
                : request.CapturedAtUtc;

            capturedAtUtc = capturedAtUtc.Kind switch
            {
                DateTimeKind.Utc => capturedAtUtc,
                DateTimeKind.Local => capturedAtUtc.ToUniversalTime(),
                _ => DateTime.SpecifyKind(capturedAtUtc, DateTimeKind.Utc)
            };

            var trackingPoint = new TeamTrackingPoint
            {
                TeamTrackingPointId = Guid.NewGuid(),
                TeamId = teamId,
                RescueBatchId = request.RescueBatchId,
                RescueOperationId = request.RescueOperationId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AccuracyMeters = request.AccuracyMeters,
                SpeedKph = request.SpeedKph,
                HeadingDegree = request.HeadingDegree,
                Source = request.Source,
                CapturedAtUtc = capturedAtUtc,
                CreatedAtUtc = DateTime.UtcNow,
                Note = request.Note
            };

            await _unitOfWork.TeamTrackingPoints.AddAsync(trackingPoint);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _rescueRequestService.RecalculateActiveBatchEtaAsync(teamId, cancellationToken);

            return new TeamTrackingPointResponse
            {
                TeamTrackingPointId = trackingPoint.TeamTrackingPointId,
                TeamId = trackingPoint.TeamId,
                RescueBatchId = trackingPoint.RescueBatchId,
                RescueOperationId = trackingPoint.RescueOperationId,
                Latitude = trackingPoint.Latitude,
                Longitude = trackingPoint.Longitude,
                AccuracyMeters = trackingPoint.AccuracyMeters,
                SpeedKph = trackingPoint.SpeedKph,
                HeadingDegree = trackingPoint.HeadingDegree,
                Source = trackingPoint.Source,
                CapturedAtUtc = trackingPoint.CapturedAtUtc,
                CreatedAtUtc = trackingPoint.CreatedAtUtc,
                Note = trackingPoint.Note
            };
        }

        public async Task<List<TeamTrackingPointResponse>> GetLatestTrackingPointsAsync(
            Guid teamId,
            int limit,
            CancellationToken cancellationToken)
        {
            var safeLimit = limit <= 0 ? 50 : Math.Min(limit, 500);

            var points = await _unitOfWork.TeamTrackingPoints.GetLatestByTeamAsync(
                teamId,
                safeLimit,
                cancellationToken);

            return points
                .OrderByDescending(x => x.CapturedAtUtc)
                .Select(x => new TeamTrackingPointResponse
                {
                    TeamTrackingPointId = x.TeamTrackingPointId,
                    TeamId = x.TeamId,
                    RescueBatchId = x.RescueBatchId,
                    RescueOperationId = x.RescueOperationId,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    AccuracyMeters = x.AccuracyMeters,
                    SpeedKph = x.SpeedKph,
                    HeadingDegree = x.HeadingDegree,
                    Source = x.Source,
                    CapturedAtUtc = x.CapturedAtUtc,
                    CreatedAtUtc = x.CreatedAtUtc,
                    Note = x.Note
                })
                .ToList();
        }

        // Helper Methods
        private async Task<TeamResponse> MapToTeamResponse(Team team, CancellationToken cancellationToken)
        {
            var totalMembers = await _unitOfWork.Teams.GetTeamMemberCountAsync(team.TeamId, cancellationToken);

            return new TeamResponse
            {
                TeamId = team.TeamId,
                Name = team.Name,
                Description = team.Description,
                ContactPhone = team.ContactPhone,
                TeamType = team.TeamType,
                TeamTypeName = team.TeamType.ToString(),
                Status = team.Status,
                ModeratorId = team.CreateBy,
                ModeratorName = team.Moderator?.DisplayName ?? team.Moderator?.UserName ?? "Unknown",
                LeaderId = team.LeaderId,
                LeaderName = team.Leader?.DisplayName ?? team.Leader?.UserName,
                TotalMembers = totalMembers,
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt
            };
        }

        private TeamDetailResponse MapToTeamDetailResponse(Team team)
        {
            var stationAssignment = team.ReliefStationTeams
                .Where(rst => rst.RemovedAt == null)
                .OrderByDescending(rst => rst.JoinedAt)
                .FirstOrDefault();

            return new TeamDetailResponse
            {
                TeamId = team.TeamId,
                Name = team.Name,
                Description = team.Description,
                ContactPhone = team.ContactPhone,
                TeamType = team.TeamType,
                TeamTypeName = team.TeamType.ToString(),
                Status = team.Status,
                ReliefStationId = stationAssignment?.ReliefStationId,
                ReliefStationName = stationAssignment?.ReliefStation?.Name,
                ReliefStationAddress = stationAssignment?.ReliefStation?.Address,
                ReliefStationStatus = stationAssignment?.ReliefStation?.ReliefStationStatus,
                Moderator = new ModeratorInfo
                {
                    UserId = team.CreateBy,
                    DisplayName = team.Moderator?.DisplayName ?? team.Moderator?.UserName ?? "Unknown",
                    Email = team.Moderator?.Email ?? ""
                },
                Leader = team.Leader != null ? new LeaderInfo
                {
                    UserId = team.Leader.Id,
                    DisplayName = team.Leader.DisplayName ?? team.Leader.UserName ?? "Unknown",
                    Email = team.Leader.Email ?? "",
                    Skills = team.Leader.VolunteerProfile?.VolunteerSkills.Select(vs => new SkillInfo
                    {
                        SkillId = vs.SkillId,
                        Code = vs.Skill.Code,
                        Name = vs.Skill.Name,
                        Description = vs.Skill.Description
                    }).ToList() ?? new List<SkillInfo>()
                } : null,
                Members = team.TeamMembers.Select(tm => new TeamMemberInfo
                {
                    UserId = tm.UserId,
                    VolunteerProfileId = tm.User.VolunteerProfile?.VolunteerProfileId,
                    DisplayName = tm.User.DisplayName ?? tm.User.UserName ?? "Unknown",
                    Email = tm.User.Email ?? "",
                    Role = tm.RoleTeam,
                    Skills = tm.User.VolunteerProfile?.VolunteerSkills.Select(vs => new SkillInfo
                    {
                        SkillId = vs.SkillId,
                        Code = vs.Skill.Code,
                        Name = vs.Skill.Name,
                        Description = vs.Skill.Description
                    }).ToList() ?? new List<SkillInfo>(),
                    JoinedAt = tm.JoinedAt
                }).ToList(),
                AssignedCampaigns = team.CampaignTeams
                    .Where(ct => !ct.IsDelete)
                    .OrderByDescending(ct => ct.AssignedAt)
                    .Select(ct => new AssignedCampaignInfo
                    {
                        CampaignId = ct.CampaignId,
                        CampaignName = ct.Campaign.Name,
                        CampaignType = (int)ct.Campaign.Type,
                        CampaignTeamId = ct.CampaignTeamId,
                        Status = (int)ct.Status,
                        Role = (int)ct.Role
                    })
                    .ToList(),
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt
            };
        }

        public async Task<List<TeamDetailResponse>> GetMyTeamsWithMembersAsync(Guid moderatorId, CancellationToken cancellationToken)
        {
            var teams = await _unitOfWork.Teams.GetTeamsByModeratorWithMembersAsync(moderatorId);
    
            return teams.Select(team => MapToTeamDetailResponse(team)).ToList();
        }

        private static void ValidateTeamType(TeamType teamType)
        {
            if (!Enum.IsDefined(typeof(TeamType), teamType))
            {
                throw new TeamValidationException("Loại đội không hợp lệ. Vui lòng chọn đội cứu trợ hoặc đội cứu hộ.");
            }
        }

    }
}
