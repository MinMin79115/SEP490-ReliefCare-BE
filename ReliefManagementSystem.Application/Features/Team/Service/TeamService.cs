using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Team.DTOs.Request;
using ReliefManagementSystem.Application.Features.Team.DTOs.Response;
using ReliefManagementSystem.Application.Features.Team.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Services
{
    public class TeamService : ITeamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeamService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<TeamResponse> CreateTeamAsync(
            CreateTeamRequest request,
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            // 1. Validate moderator role
            var moderator = await _userManager.FindByIdAsync(moderatorId.ToString());
            if (moderator == null)
                throw new Exception("Không tìm thấy người điều phối");

            var isModerator = await _userManager.IsInRoleAsync(moderator, "Moderator");
            if (!isModerator)
                throw new Exception("Chỉ có người điều phối mới có thể tạo đội");

            // 2. Validate leader nếu có
            if (request.LeaderId.HasValue)
            {
                var leader = await _unitOfWork.Users.GetByIdWithVolunteerProfileAsync(
                    request.LeaderId.Value, cancellationToken);

                if (leader == null)
                    throw new Exception("Không tìm thấy trưởng nhóm");

                if (leader.VolunteerProfile?.VerificationStatus != VerificationStatus.Approved)
                    throw new Exception("Trưởng nhóm phải là tình nguyện viên đã được xác minh");
            }

            // 3. Tạo Team
            var team = new Team
            {
                Name = request.Name,
                Description = request.Description,
                ModeratorId = moderatorId,
                LeaderId = request.LeaderId,
                Status = TeamStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Teams.AddAsync(team);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 4. Nếu có leader, tạo TeamMember
            if (request.LeaderId.HasValue)
            {
                var teamMember = new TeamMember
                {
                    TeamId = team.TeamId,
                    UserId = request.LeaderId.Value,
                    RoleTeam = TeamRole.Leader,
                    JoinedAt = DateTime.UtcNow
                };

                await _unitOfWork.TeamMembers.AddAsync(teamMember);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // 5. Return response
            return await MapToTeamResponse(team, cancellationToken);
        }

        public async Task<TeamDetailResponse> GetTeamByIdAsync(
            Guid teamId,
            CancellationToken cancellationToken)
        {
            var team = await _unitOfWork.Teams.GetByIdWithDetailsAsync(teamId);

            if (team == null)
                throw new Exception("Không tìm thấy đội");

            return MapToTeamDetailResponse(team);
        }

        public async Task<TeamResponse> UpdateTeamAsync(
            Guid teamId,
            UpdateTeamRequest request,
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            // 1. Load team
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);

            if (team == null)
                throw new Exception("Không tìm thấy đội");

            // 2. Validate moderator
            if (team.ModeratorId != moderatorId)
                throw new Exception("Chỉ có người điều phối team mới có thể chỉnh sửa");

            // 3. Handle leader change
            if (request.LeaderId != team.LeaderId)
            {
                // Validate new leader if set
                if (request.LeaderId.HasValue)
                {
                    var newLeader = await _unitOfWork.Users.GetByIdWithVolunteerProfileAsync(
                        request.LeaderId.Value, cancellationToken);

                    if (newLeader == null)
                        throw new Exception("Không tìm thấy trưởng nhóm mới");

                    if (newLeader.VolunteerProfile?.VerificationStatus != VerificationStatus.Approved)
                        throw new Exception("Trưởng nhóm mới phải là tình nguyện viên đã được xác minh");

                    // Check if new leader is already a member
                    var isNewLeaderMember = await _unitOfWork.TeamMembers.IsMemberAsync(teamId, request.LeaderId.Value);

                    if (!isNewLeaderMember)
                    {
                        // Add as new member with Leader role
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
                        // Update existing member to Leader role
                        var existingMember = await _unitOfWork.TeamMembers.GetByTeamAndUserAsync(teamId, request.LeaderId.Value);
                        if (existingMember != null)
                        {
                            existingMember.RoleTeam = TeamRole.Leader;
                            await _unitOfWork.TeamMembers.UpdateAsync(existingMember);
                        }
                    }
                }

                // Demote old leader to Member if exists
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

            // 4. Update basic info
            team.Name = request.Name;
            team.Description = request.Description;
            team.Status = request.Status;
            team.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Teams.UpdateAsync(team);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await MapToTeamResponse(team, cancellationToken);
        }

        public async Task<bool> DeleteTeamAsync(
            Guid teamId,
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);

            if (team == null)
                throw new Exception("Không tìm thấy đội");

            if (team.ModeratorId != moderatorId)
                throw new Exception("Chỉ người điều phối đội mới được xoá");

            await _unitOfWork.Teams.DeleteAsync(team);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
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

        public async Task<Pagination<TeamResponse>> SearchTeamsAsync(
            SearchTeamRequest request,
            CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Teams.GetQueryable();

            // Apply filters
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

            // Order by
            query = query.OrderByDescending(t => t.CreatedAt);

            // Apply pagination
            var pagedTeams = await Pagination<Team>.ToPagedList(query, request.PageIndex, request.PageSize);

            // Map to response
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

        public async Task<bool> RemoveMemberAsync(
            Guid teamId,
            Guid userId,
            Guid moderatorId,
            CancellationToken cancellationToken)
        {
            // Validate team and moderator
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            if (team == null)
                throw new Exception("Không tìm thấy đội");

            if (team.ModeratorId != moderatorId)
                throw new Exception("Chỉ điều phối đội mới có thể xoá thành viên");

            // Cannot remove leader
            if (team.LeaderId == userId)
                throw new Exception("Không thể xoá trưởng nhóm hiện tại. Phải đổi trưởng nhóm trước.");

            // Get and remove member
            var member = await _unitOfWork.TeamMembers.GetByTeamAndUserAsync(teamId, userId);
            if (member == null)
                throw new Exception("Thành viên không tồn tại trong đội");

            await _unitOfWork.TeamMembers.DeleteAsync(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
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
    }
}
