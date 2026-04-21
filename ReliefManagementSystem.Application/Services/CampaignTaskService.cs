using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests;
using ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Responses;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    public class CampaignTaskService : ICampaignTaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CampaignTaskService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<CampaignTaskResponse> CreateAsync(Guid campaignId, CreateCampaignTaskRequest request, CancellationToken cancellationToken = default)
        {
            var campaign = await GetReliefCampaignAsync(campaignId, cancellationToken);
            var campaignTeam = await GetCampaignTeamAsync(campaignId, request.CampaignTeamId, cancellationToken);

            ValidateSchedule(request.StartDate, request.DueDate);

            var task = new CampaignTask
            {
                CampaignTaskId = Guid.NewGuid(),
                CampaignTeamId = campaignTeam.CampaignTeamId,
                Title = request.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                StartDate = request.StartDate,
                DueDate = request.DueDate,
                CreatedBy = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated."),
                Status = CampaignTaskStatus.Planned,
                Priority = request.Priority,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.CampaignTasks.AddAsync(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapSummary(task, campaign.CampaignId, campaignTeam.Team?.Name);
        }

        public async Task<Pagination<CampaignTaskResponse>> GetPagedAsync(Guid campaignId, CampaignTaskListQueryRequest request, CancellationToken cancellationToken = default)
        {
            await GetReliefCampaignAsync(campaignId, cancellationToken);

            if (request.CampaignTeamId.HasValue)
            {
                await GetCampaignTeamAsync(campaignId, request.CampaignTeamId.Value, cancellationToken);
            }

            var (items, totalCount) = await _unitOfWork.CampaignTasks.GetPagedByCampaignAsync(
                campaignId,
                request.PageIndex,
                request.PageSize,
                request.Status,
                request.CampaignTeamId,
                cancellationToken);

            var mapped = items.Select(x => MapSummary(x, campaignId, x.CampaignTeam?.Team?.Name)).ToList();
            return new Pagination<CampaignTaskResponse>(mapped, totalCount, request.PageIndex, request.PageSize);
        }

        public async Task<Pagination<MyMemberTaskResponse>> GetMyMemberTasksAsync(Guid campaignId, MyMemberTaskQueryRequest request, CancellationToken cancellationToken = default)
        {
            await GetReliefCampaignAsync(campaignId, cancellationToken);

            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var volunteerProfile = await _unitOfWork.VolunteerProfiles.GetByUserIdAsync(currentUserId, cancellationToken)
                ?? throw new KeyNotFoundException("Volunteer profile for current user was not found.");

            var query = _unitOfWork.MemberTasks.GetQueryable()
                .Where(x => x.VolunteerProfileId == volunteerProfile.VolunteerProfileId)
                .Where(x => x.CampaignTask.CampaignTeam.CampaignId == campaignId);

            if (request.Status.HasValue)
                query = query.Where(x => x.Status == request.Status.Value);

            if (request.CampaignTeamId.HasValue)
                query = query.Where(x => x.CampaignTask.CampaignTeamId == request.CampaignTeamId.Value);

            query = query.OrderByDescending(x => x.AssignedAt);

            var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
            var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var mapped = items.Select(x => new MyMemberTaskResponse
            {
                MemberTaskId = x.MemberTaskId,
                CampaignTaskId = x.CampaignTaskId,
                CampaignId = x.CampaignTask.CampaignTeam.CampaignId,
                CampaignTeamId = x.CampaignTask.CampaignTeamId,
                CampaignTeamName = x.CampaignTask.CampaignTeam.Team?.Name ?? string.Empty,
                CampaignTaskTitle = x.CampaignTask.Title,
                CampaignTaskDescription = x.CampaignTask.Description,
                StartDate = x.CampaignTask.StartDate,
                DueDate = x.CampaignTask.DueDate,
                CampaignTaskStatus = x.CampaignTask.Status,
                Priority = x.CampaignTask.Priority,
                VolunteerProfileId = x.VolunteerProfileId,
                VolunteerName = ResolveVolunteerDisplayName(x.VolunteerProfile),
                SubTaskTitle = x.SubTaskTitle,
                TaskNote = x.TaskNote,
                AssignedAt = x.AssignedAt,
                CompletedAt = x.CompletedAt,
                Status = x.Status,
            }).ToList();

            return new Pagination<MyMemberTaskResponse>(mapped, totalCount, pageIndex, pageSize);
        }

        public async Task<CampaignTaskDetailResponse> GetByIdAsync(Guid campaignTaskId, CancellationToken cancellationToken = default)
        {
            var task = await _unitOfWork.CampaignTasks.GetByIdWithDetailsAsync(campaignTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign task '{campaignTaskId}' was not found.");

            var campaign = await GetReliefCampaignAsync(task.CampaignTeam.CampaignId, cancellationToken);

            return MapDetail(task, campaign.CampaignId, task.CampaignTeam.Team?.Name);
        }

        public async Task<CampaignTaskResponse> UpdateAsync(Guid campaignTaskId, UpdateCampaignTaskRequest request, CancellationToken cancellationToken = default)
        {
            var task = await _unitOfWork.CampaignTasks.GetByIdWithDetailsAsync(campaignTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign task '{campaignTaskId}' was not found.");

            var campaign = await GetReliefCampaignAsync(task.CampaignTeam.CampaignId, cancellationToken);
            ValidateTaskEditable(task.Status);
            ValidateSchedule(request.StartDate, request.DueDate);

            task.Title = request.Title.Trim();
            task.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            task.StartDate = request.StartDate;
            task.DueDate = request.DueDate;
            task.Priority = request.Priority;

            await _unitOfWork.CampaignTasks.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapSummary(task, campaign.CampaignId, task.CampaignTeam.Team?.Name);
        }

        public async Task<CampaignTaskResponse> ChangeStatusAsync(Guid campaignTaskId, ChangeCampaignTaskStatusRequest request, CancellationToken cancellationToken = default)
        {
            var task = await _unitOfWork.CampaignTasks.GetByIdWithDetailsAsync(campaignTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign task '{campaignTaskId}' was not found.");

            var campaign = await GetReliefCampaignAsync(task.CampaignTeam.CampaignId, cancellationToken);
            ValidateTaskStatusTransition(task.Status, request.Status);
            ValidateExecutionStateAllowed(campaign, request.Status);

            task.Status = request.Status;

            await _unitOfWork.CampaignTasks.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapSummary(task, campaign.CampaignId, task.CampaignTeam.Team?.Name);
        }

        public async Task<MemberTaskResponse> AssignMemberAsync(Guid campaignTaskId, AssignMemberTaskRequest request, CancellationToken cancellationToken = default)
        {
            var task = await _unitOfWork.CampaignTasks.GetByIdWithDetailsAsync(campaignTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign task '{campaignTaskId}' was not found.");

            await GetReliefCampaignAsync(task.CampaignTeam.CampaignId, cancellationToken);
            ValidateTaskEditable(task.Status);

            var volunteerProfile = await _unitOfWork.VolunteerProfiles.GetByIdWithSkillsAndUserAsync(request.VolunteerProfileId)
                ?? throw new KeyNotFoundException($"Volunteer profile '{request.VolunteerProfileId}' was not found.");

            var isMember = await _unitOfWork.TeamMembers.IsMemberAsync(task.CampaignTeam.TeamId, volunteerProfile.UserId);
            if (!isMember)
            {
                throw new InvalidOperationException("Assigned volunteer must belong to the owning campaign team.");
            }

            var memberTask = new MemberTask
            {
                MemberTaskId = Guid.NewGuid(),
                CampaignTaskId = task.CampaignTaskId,
                VolunteerProfileId = volunteerProfile.VolunteerProfileId,
                SubTaskTitle = request.SubTaskTitle.Trim(),
                TaskNote = string.IsNullOrWhiteSpace(request.TaskNote) ? null : request.TaskNote.Trim(),
                AssignedAt = DateTime.UtcNow,
                Status = MemberTaskStatus.Assigned
            };

            await _unitOfWork.MemberTasks.AddAsync(memberTask);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapMemberTask(memberTask, ResolveVolunteerDisplayName(volunteerProfile));
        }

        public async Task<List<MemberTaskResponse>> BulkAssignMembersAsync(Guid campaignTaskId, BulkAssignMembersTaskRequest request, CancellationToken cancellationToken = default)
        {
            var task = await _unitOfWork.CampaignTasks.GetByIdWithDetailsAsync(campaignTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign task '{campaignTaskId}' was not found.");

            await GetReliefCampaignAsync(task.CampaignTeam.CampaignId, cancellationToken);
            ValidateTaskEditable(task.Status);

            if (request.Members is null || !request.Members.Any())
            {
                throw new InvalidOperationException("At least one member task assignment is required.");
            }

            var results = new List<MemberTaskResponse>();
            var memberTasks = new List<MemberTask>();
          
            foreach (var memberRequest in request.Members)
            {
                var volunteerProfile = await _unitOfWork.VolunteerProfiles.GetByIdWithSkillsAndUserAsync(memberRequest.VolunteerProfileId)
                    ?? throw new KeyNotFoundException($"Volunteer profile '{memberRequest.VolunteerProfileId}' was not found.");

                var isMember = await _unitOfWork.TeamMembers.IsMemberAsync(task.CampaignTeam.TeamId, volunteerProfile.UserId);
                if (!isMember)
                {
                    throw new InvalidOperationException("Assigned volunteer must belong to the owning campaign team.");
                }

                var memberTask = new MemberTask
                {
                    MemberTaskId = Guid.NewGuid(),
                    CampaignTaskId = task.CampaignTaskId,
                    VolunteerProfileId = volunteerProfile.VolunteerProfileId,
                    SubTaskTitle = memberRequest.SubTaskTitle.Trim(),
                    TaskNote = string.IsNullOrWhiteSpace(memberRequest.TaskNote) ? null : memberRequest.TaskNote.Trim(),
                    AssignedAt = DateTime.UtcNow,
                    Status = MemberTaskStatus.Assigned
                };

                memberTasks.Add(memberTask);
            }

            foreach (var memberTask in memberTasks)
            {
                await _unitOfWork.MemberTasks.AddAsync(memberTask);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var memberTask in memberTasks)
            {
                var volunteerProfile = await _unitOfWork.VolunteerProfiles.GetByIdWithSkillsAndUserAsync(memberTask.VolunteerProfileId);
                results.Add(MapMemberTask(memberTask, ResolveVolunteerDisplayName(volunteerProfile)));
            }

            return results;
        }

        public async Task<MemberTaskResponse> ChangeMemberTaskStatusAsync(Guid memberTaskId, ChangeMemberTaskStatusRequest request, CancellationToken cancellationToken = default)
        {
            var memberTask = await _unitOfWork.MemberTasks.GetByIdAsync(memberTaskId)
                ?? throw new KeyNotFoundException($"Member task '{memberTaskId}' was not found.");

            var task = await _unitOfWork.CampaignTasks.GetByIdWithDetailsAsync(memberTask.CampaignTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign task '{memberTask.CampaignTaskId}' was not found.");

            await GetReliefCampaignAsync(task.CampaignTeam.CampaignId, cancellationToken);
            ValidateMemberTaskStatusTransition(memberTask.Status, request.Status);

            memberTask.Status = request.Status;

            if (request.Status == MemberTaskStatus.Completed)
            {
                memberTask.CompletedAt = DateTime.UtcNow;
            }

            await _unitOfWork.MemberTasks.UpdateAsync(memberTask);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Auto-transition parent task based on subtask progress
            await AutoUpdateParentTaskStatusAsync(task, cancellationToken);

            var volunteerProfile = await _unitOfWork.VolunteerProfiles.GetByIdWithSkillsAndUserAsync(memberTask.VolunteerProfileId);
            return MapMemberTask(memberTask, ResolveVolunteerDisplayName(volunteerProfile));
        }

        private async Task AutoUpdateParentTaskStatusAsync(CampaignTask task, CancellationToken cancellationToken)
        {
            // Reload member tasks to get fresh statuses
            var memberTasks = await _unitOfWork.MemberTasks.GetByCampaignTaskIdAsync(task.CampaignTaskId, cancellationToken);
            if (memberTasks.Count == 0) return;

            var allTerminal = memberTasks.All(mt => mt.Status is MemberTaskStatus.Completed or MemberTaskStatus.Cancelled);
            var anyInProgress = memberTasks.Any(mt => mt.Status is MemberTaskStatus.InProgress);
            var anyFailed = memberTasks.Any(mt => mt.Status is MemberTaskStatus.Failed);

            CampaignTaskStatus? newStatus = null;

            if (allTerminal && task.Status != CampaignTaskStatus.Completed)
            {
                // All subtasks done → auto-complete parent
                newStatus = CampaignTaskStatus.Completed;
            }
            else if (anyFailed && task.Status == CampaignTaskStatus.InProgress)
            {
                // A subtask failed → block parent
                newStatus = CampaignTaskStatus.Blocked;
            }
            else if (anyInProgress && task.Status == CampaignTaskStatus.Planned)
            {
                // First subtask started → auto-start parent
                newStatus = CampaignTaskStatus.InProgress;
            }

            if (newStatus.HasValue && newStatus.Value != task.Status)
            {
                task.Status = newStatus.Value;
                await _unitOfWork.CampaignTasks.UpdateAsync(task);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteAsync(Guid campaignTaskId, CancellationToken cancellationToken = default)
        {
            var task = await _unitOfWork.CampaignTasks.GetByIdWithDetailsAsync(campaignTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign task '{campaignTaskId}' was not found.");

            ValidateTaskDeletable(task.Status);

            await _unitOfWork.CampaignTasks.DeleteAsync(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<Campaign> GetReliefCampaignAsync(Guid campaignId, CancellationToken cancellationToken)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithDetailsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            if (campaign.Type != CampaignType.Relief)
            {
                throw new InvalidOperationException("Campaign tasks are only supported for relief campaigns.");
            }

            return campaign;
        }

        private async Task<CampaignTeam> GetCampaignTeamAsync(Guid campaignId, Guid campaignTeamId, CancellationToken cancellationToken)
        {
            var teams = await _unitOfWork.Campaigns.GetCampaignTeamsAsync(campaignId, cancellationToken);
            var campaignTeam = teams.FirstOrDefault(x => x.CampaignTeamId == campaignTeamId);
            if (campaignTeam is null)
            {
                throw new KeyNotFoundException($"Campaign team '{campaignTeamId}' was not found in campaign '{campaignId}'.");
            }

            return campaignTeam;
        }

        private static void ValidateSchedule(DateTime startDate, DateTime? dueDate)
        {
            if (dueDate.HasValue && dueDate.Value < startDate)
            {
                throw new InvalidOperationException("Due date must be greater than or equal to start date.");
            }
        }

        private static void ValidateTaskEditable(CampaignTaskStatus status)
        {
            if (status is CampaignTaskStatus.Completed or CampaignTaskStatus.Cancelled)
            {
                throw new InvalidOperationException("Completed or cancelled tasks cannot be updated.");
            }
        }

        private static void ValidateTaskDeletable(CampaignTaskStatus status)
        {
            if (status is CampaignTaskStatus.InProgress or CampaignTaskStatus.Blocked or CampaignTaskStatus.Completed)
            {
                throw new InvalidOperationException("Only planned or cancelled tasks can be deleted.");
            }
        }

        private static void ValidateTaskStatusTransition(CampaignTaskStatus current, CampaignTaskStatus next)
        {
            if (current == next)
            {
                return;
            }

            var valid = current switch
            {
                CampaignTaskStatus.Planned => next is CampaignTaskStatus.InProgress or CampaignTaskStatus.Cancelled,
                CampaignTaskStatus.InProgress => next is CampaignTaskStatus.Blocked or CampaignTaskStatus.Completed or CampaignTaskStatus.Cancelled,
                CampaignTaskStatus.Blocked => next is CampaignTaskStatus.InProgress or CampaignTaskStatus.Cancelled,
                _ => false
            };

            if (!valid)
            {
                throw new InvalidOperationException($"Invalid campaign task status transition: {current} -> {next}.");
            }
        }

        private static void ValidateMemberTaskStatusTransition(MemberTaskStatus current, MemberTaskStatus next)
        {
            if (current == next)
            {
                return;
            }

            var valid = current switch
            {
                MemberTaskStatus.Assigned => next is MemberTaskStatus.InProgress or MemberTaskStatus.Cancelled,
                MemberTaskStatus.InProgress => next is MemberTaskStatus.Completed or MemberTaskStatus.Failed or MemberTaskStatus.Cancelled,
                MemberTaskStatus.Failed => next is MemberTaskStatus.InProgress,
                _ => false
            };

            if (!valid)
            {
                throw new InvalidOperationException($"Invalid member task status transition: {current} -> {next}.");
            }
        }

        private static void ValidateExecutionStateAllowed(Campaign campaign, CampaignTaskStatus next)
        {
            if (next is CampaignTaskStatus.InProgress or CampaignTaskStatus.Blocked or CampaignTaskStatus.Completed)
            {
                if (campaign.Status != CampaignStatus.Active)
                {
                    throw new InvalidOperationException("Task execution transitions are only allowed when the parent relief campaign is Active.");
                }
            }
        }

        private static CampaignTaskResponse MapSummary(CampaignTask task, Guid campaignId, string? teamName)
            => new()
            {
                CampaignTaskId = task.CampaignTaskId,
                CampaignId = campaignId,
                CampaignTeamId = task.CampaignTeamId,
                CampaignTeamName = teamName ?? string.Empty,
                Title = task.Title,
                Description = task.Description,
                StartDate = task.StartDate,
                DueDate = task.DueDate,
                Status = task.Status,
                Priority = task.Priority,
                CreatedBy = task.CreatedBy,
                CreatedAt = task.CreatedAt
            };

        private static CampaignTaskDetailResponse MapDetail(CampaignTask task, Guid campaignId, string? teamName)
            => new()
            {
                CampaignTaskId = task.CampaignTaskId,
                CampaignId = campaignId,
                CampaignTeamId = task.CampaignTeamId,
                CampaignTeamName = teamName ?? string.Empty,
                Title = task.Title,
                Description = task.Description,
                StartDate = task.StartDate,
                DueDate = task.DueDate,
                Status = task.Status,
                Priority = task.Priority,
                CreatedBy = task.CreatedBy,
                CreatedAt = task.CreatedAt,
                MemberTaskCount = task.MemberTasks.Count,
                CompletedMemberTaskCount = task.MemberTasks.Count(x => x.Status == MemberTaskStatus.Completed),
                MemberTasks = task.MemberTasks.Select(x => MapMemberTask(x, ResolveVolunteerDisplayName(x.VolunteerProfile))).ToList()
            };

        private static MemberTaskResponse MapMemberTask(MemberTask memberTask, string? volunteerName)
            => new()
            {
                MemberTaskId = memberTask.MemberTaskId,
                CampaignTaskId = memberTask.CampaignTaskId,
                VolunteerProfileId = memberTask.VolunteerProfileId,
                VolunteerName = volunteerName ?? string.Empty,
                SubTaskTitle = memberTask.SubTaskTitle,
                TaskNote = memberTask.TaskNote,
                AssignedAt = memberTask.AssignedAt,
                CompletedAt = memberTask.CompletedAt,
                Status = memberTask.Status
            };

        private static string ResolveVolunteerDisplayName(VolunteerProfile? volunteerProfile)
            => volunteerProfile?.User?.DisplayName
                ?? volunteerProfile?.User?.UserName
                ?? volunteerProfile?.User?.Email
                ?? string.Empty;
    }
}
