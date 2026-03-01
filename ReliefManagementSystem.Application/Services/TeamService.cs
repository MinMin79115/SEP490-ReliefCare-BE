
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

namespace ReliefManagementSystem.Application.Services
{
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TeamService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //Create a team
        public async Task<TeamResponse> CreateTeamAsync(
            CreateTeamRequest request,
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            var team = new Team
            {
                Name = request.Name,
                Description = request.Description,
                ModeratorId = moderatorId,
                LeaderId = null, 
                Status = TeamStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Teams.AddAsync(team);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);

            if (team == null)
                throw new TeamNotFoundException();

            if (team.ModeratorId != moderatorId)
                throw new UnauthorizedTeamActionException("chỉnh sửa");

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

            team.Name = request.Name;
            team.Description = request.Description;
            team.Status = request.Status;
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

            if (team.ModeratorId != moderatorId)
                throw new UnauthorizedTeamActionException("xoá");

            await _unitOfWork.Teams.DeleteAsync(team);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
        
        //Get team list
        public async Task<List<TeamResponse>> GetAllTeamsAsync(CancellationToken cancellationToken)
        {
            var teams = await _unitOfWork.Teams.GetAllAsync();

            var responses = new List<TeamResponse>();
            foreach (var team in teams)
            {
                responses.Add(await MapToTeamResponse(team, cancellationToken));
            }

            return responses;
        }

        //Search team
        public async Task<Pagination<TeamResponse>> SearchTeamsAsync(
            SearchTeamRequest request,
            CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Teams.GetQueryable();

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(t => t.Name.Contains(request.Name));
            }

            if (request.Status.HasValue)
            {
                query = query.Where(t => t.Status == request.Status.Value);
            }

            if (request.ModeratorId.HasValue)
            {
                query = query.Where(t => t.ModeratorId == request.ModeratorId.Value);
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

            if (team.ModeratorId != moderatorId)
                throw new UnauthorizedTeamActionException("xoá thành viên");

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

            if (team.ModeratorId != moderatorId)
                throw new UnauthorizedTeamActionException("thêm thành viên");

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

        //Update role for member role to leader role
        public async Task<TeamMemberResponse> PromoteMemberToLeaderAsync(
            Guid teamId,
            Guid userId,
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            var team = await _unitOfWork.Teams.GetByIdWithDetailsAsync(teamId);
            
            if (team == null)
                throw new TeamNotFoundException();

            if (team.ModeratorId != moderatorId)
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

        // Helper Methods
        private async Task<TeamResponse> MapToTeamResponse(Team team, CancellationToken cancellationToken)
        {
            var totalMembers = await _unitOfWork.Teams.GetTeamMemberCountAsync(team.TeamId, cancellationToken);

            return new TeamResponse
            {
                TeamId = team.TeamId,
                Name = team.Name,
                Description = team.Description,
                Status = team.Status,
                ModeratorId = team.ModeratorId,
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
            return new TeamDetailResponse
            {
                TeamId = team.TeamId,
                Name = team.Name,
                Description = team.Description,
                Status = team.Status,
                Moderator = new ModeratorInfo
                {
                    UserId = team.ModeratorId,
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
                CreatedAt = team.CreatedAt,
                UpdatedAt = team.UpdatedAt
            };
        }

        public async Task<List<TeamDetailResponse>> GetMyTeamsWithMembersAsync(Guid moderatorId, CancellationToken cancellationToken)
        {
            var teams = await _unitOfWork.Teams.GetTeamsByModeratorWithMembersAsync(moderatorId);
    
            return teams.Select(team => MapToTeamDetailResponse(team)).ToList();
        }
    }
}
