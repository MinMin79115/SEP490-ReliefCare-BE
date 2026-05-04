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
        private const string FailureReasonMarker = "\n[FAILURE_REASON] ";

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
            await EnsureTeamLeaderOrCoordinatorAsync(campaignTeam, cancellationToken);

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

        public async Task<List<AdminCampaignTaskAggregateResponse>> GetAdminTaskAggregateAsync(DateTime? from = null, DateTime? to = null, Guid? teamId = null, Guid? campaignId = null, CancellationToken cancellationToken = default)
        {
            var query = _unitOfWork.CampaignTasks.GetQueryable()
                .AsNoTracking()
                .Where(x => !x.CampaignTeam.IsDelete && x.CampaignTeam.Campaign.Type == CampaignType.Relief)
                .Include(x => x.CampaignTeam)
                    .ThenInclude(x => x.Campaign)
                .Include(x => x.CampaignTeam)
                    .ThenInclude(x => x.Team)
                        .ThenInclude(x => x.TeamMembers)
                .Include(x => x.MemberTasks)
                    .ThenInclude(x => x.VolunteerProfile)
                        .ThenInclude(x => x.User)
                .AsQueryable();

            if (from.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= from.Value || x.StartDate >= from.Value || (x.DueDate.HasValue && x.DueDate.Value >= from.Value));
            }

            if (to.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= to.Value || x.StartDate <= to.Value || (x.DueDate.HasValue && x.DueDate.Value <= to.Value));
            }

            if (teamId.HasValue && teamId.Value != Guid.Empty)
            {
                query = query.Where(x => x.CampaignTeam.TeamId == teamId.Value);
            }

            if (campaignId.HasValue && campaignId.Value != Guid.Empty)
            {
                query = query.Where(x => x.CampaignTeam.CampaignId == campaignId.Value);
            }

            var tasks = await query
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return tasks.Select(task => new AdminCampaignTaskAggregateResponse
            {
                CampaignTaskId = task.CampaignTaskId,
                CampaignId = task.CampaignTeam.CampaignId,
                CampaignTeamId = task.CampaignTeamId,
                CampaignTeamName = task.CampaignTeam.Team?.Name ?? string.Empty,
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
                MemberTasks = task.MemberTasks.Select(x => MapMemberTask(x, ResolveVolunteerDisplayName(x.VolunteerProfile))).ToList(),
                TeamId = task.CampaignTeam.TeamId,
                TeamName = task.CampaignTeam.Team?.Name ?? string.Empty,
                TeamType = task.CampaignTeam.Team?.TeamType.ToString() ?? string.Empty,
                TeamMemberCount = task.CampaignTeam.Team?.TeamMembers.Count ?? 0,
                CampaignName = task.CampaignTeam.Campaign?.Name ?? string.Empty,
                CampaignStatus = task.CampaignTeam.Campaign?.Status.ToString() ?? string.Empty,
            }).ToList();
        }

        public async Task<List<AdminTopTeamResponse>> GetAdminTopTeamsAsync(DateTime? from = null, DateTime? to = null, Guid? teamId = null, Guid? campaignId = null, int top = 4, CancellationToken cancellationToken = default)
        {
            var tasks = await GetAdminTaskAggregateAsync(from, to, teamId, campaignId, cancellationToken);
            var limit = top <= 0 ? 4 : Math.Min(top, 20);

            return tasks
                .GroupBy(task => new { task.TeamId, task.TeamName, task.TeamType })
                .Select(group =>
                {
                    var memberTasks = group.SelectMany(task => task.MemberTasks).ToList();
                    var volunteerScores = memberTasks
                        .GroupBy(task => new { task.VolunteerProfileId, task.VolunteerName })
                        .Select(item => new
                        {
                            item.Key.VolunteerName,
                            Score = item.Sum(task => task.Status == MemberTaskStatus.Completed ? 2 : task.Status == MemberTaskStatus.InProgress ? 1 : 0),
                        })
                        .OrderByDescending(item => item.Score)
                        .ThenBy(item => item.VolunteerName)
                        .FirstOrDefault();

                    var completedMemberTasks = memberTasks.Count(task => task.Status == MemberTaskStatus.Completed);
                    var inProgressMemberTasks = memberTasks.Count(task => task.Status == MemberTaskStatus.InProgress);
                    var failedMemberTasks = memberTasks.Count(task => task.Status == MemberTaskStatus.Failed);

                    return new AdminTopTeamResponse
                    {
                        TeamId = group.Key.TeamId,
                        TeamName = group.Key.TeamName,
                        TeamType = group.Key.TeamType,
                        CampaignId = group.Select(task => task.CampaignId).FirstOrDefault(),
                        CampaignName = group.Select(task => task.CampaignName).FirstOrDefault() ?? string.Empty,
                        TeamMemberCount = group.Max(task => task.TeamMemberCount),
                        TaskCount = group.Count(),
                        MemberTaskCount = memberTasks.Count,
                        CompletedMemberTaskCount = completedMemberTasks,
                        InProgressMemberTaskCount = inProgressMemberTasks,
                        FailedMemberTaskCount = failedMemberTasks,
                        TopVolunteerName = volunteerScores?.VolunteerName,
                        TopVolunteerScore = volunteerScores?.Score ?? 0,
                        LatestTaskDate = group.Max(task => task.CreatedAt),
                        ImpactScore = completedMemberTasks * 3m + inProgressMemberTasks * 2m + group.Count(),
                    };
                })
                .OrderByDescending(item => item.ImpactScore)
                .ThenByDescending(item => item.CompletedMemberTaskCount)
                .ThenByDescending(item => item.TeamMemberCount)
                .Take(limit)
                .ToList();
        }

        public async Task<Pagination<MyMemberTaskResponse>> GetMyMemberTasksAsync(Guid campaignId, MyMemberTaskQueryRequest request, CancellationToken cancellationToken = default)
        {
            await GetReliefCampaignAsync(campaignId, cancellationToken);

            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var volunteerProfile = await _unitOfWork.VolunteerProfiles.GetByUserIdAsync(currentUserId)
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

            var mapped = items.Select(x =>
            {
                var noteParts = SplitTaskNoteAndFailureReason(x.TaskNote);
                return new MyMemberTaskResponse
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
                    TaskNote = noteParts.TaskNote,
                    FailureReason = noteParts.FailureReason,
                    AssignedAt = x.AssignedAt,
                    CompletedAt = x.CompletedAt,
                    Status = x.Status,
                    Deliveries = x.MemberTaskDeliveries.Select(MapMemberTaskDelivery).ToList(),
                };
            }).ToList();

            return new Pagination<MyMemberTaskResponse>(mapped, totalCount, pageIndex, pageSize);
        }

        public async Task<List<MemberTaskDeliveryResponse>> GetMyMemberTaskDeliveriesAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            await GetReliefCampaignAsync(campaignId, cancellationToken);

            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var volunteerProfile = await _unitOfWork.VolunteerProfiles.GetByUserIdAsync(currentUserId)
                ?? throw new KeyNotFoundException("Volunteer profile for current user was not found.");

            var deliveries = await _unitOfWork.MemberTaskDeliveries.GetQueryable()
                .Where(x => x.MemberTask.CampaignTask.CampaignTeam.CampaignId == campaignId)
                .Where(x => x.AssignedVolunteerProfileId == volunteerProfile.VolunteerProfileId ||
                            (x.AssignedVolunteerProfileId == null && x.MemberTask.VolunteerProfileId == volunteerProfile.VolunteerProfileId))
                .OrderBy(x => x.HouseholdDelivery.ScheduledAt)
                .ToListAsync(cancellationToken);

            return deliveries.Select(MapMemberTaskDelivery).ToList();
        }

        public async Task<CampaignTaskDetailResponse> GetByIdAsync(Guid campaignTaskId, CancellationToken cancellationToken = default)
        {
            var task = await _unitOfWork.CampaignTasks.GetByIdWithDetailsAsync(campaignTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign task '{campaignTaskId}' was not found.");

            var campaign = await GetReliefCampaignAsync(task.CampaignTeam.CampaignId, cancellationToken);
            await EnsureCanAccessCampaignTaskAsync(task, cancellationToken);

            return MapDetail(task, campaign.CampaignId, task.CampaignTeam.Team?.Name);
        }

        public async Task<CampaignTaskResponse> UpdateAsync(Guid campaignTaskId, UpdateCampaignTaskRequest request, CancellationToken cancellationToken = default)
        {
            var task = await _unitOfWork.CampaignTasks.GetByIdWithDetailsAsync(campaignTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign task '{campaignTaskId}' was not found.");

            var campaign = await GetReliefCampaignAsync(task.CampaignTeam.CampaignId, cancellationToken);
            await EnsureTeamLeaderOrCoordinatorAsync(task.CampaignTeam, cancellationToken);
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
            await EnsureTeamLeaderOrCoordinatorAsync(task.CampaignTeam, cancellationToken);
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
            await EnsureTeamLeaderOrCoordinatorAsync(task.CampaignTeam, cancellationToken);
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
            await EnsureTeamLeaderOrCoordinatorAsync(task.CampaignTeam, cancellationToken);
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

        public async Task<List<MemberTaskResponse>> CreateMemberTasksFromHouseholdsAsync(Guid campaignTaskId, CreateMemberTaskFromHouseholdsRequest request, CancellationToken cancellationToken = default)
        {
            var task = await _unitOfWork.CampaignTasks.GetByIdWithDetailsAsync(campaignTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign task '{campaignTaskId}' was not found.");

            await GetReliefCampaignAsync(task.CampaignTeam.CampaignId, cancellationToken);
            await EnsureTeamLeaderOrCoordinatorAsync(task.CampaignTeam, cancellationToken);
            ValidateTaskEditable(task.Status);

            if (request.HouseholdDeliveryIds is null || request.HouseholdDeliveryIds.Count == 0)
                throw new InvalidOperationException("At least one household delivery is required.");

            var volunteerProfile = await _unitOfWork.VolunteerProfiles.GetByIdWithSkillsAndUserAsync(request.VolunteerProfileId)
                ?? throw new KeyNotFoundException($"Volunteer profile '{request.VolunteerProfileId}' was not found.");

            var isMember = await _unitOfWork.TeamMembers.IsMemberAsync(task.CampaignTeam.TeamId, volunteerProfile.UserId);
            if (!isMember)
                throw new InvalidOperationException("Assigned volunteer must belong to the owning campaign team.");

            var deliveries = await _unitOfWork.HouseholdDeliveries.GetQueryable()
                .Include(x => x.CampaignHousehold)
                .Where(x => request.HouseholdDeliveryIds.Contains(x.HouseholdDeliveryId))
                .ToListAsync(cancellationToken);

            if (deliveries.Count != request.HouseholdDeliveryIds.Count)
                throw new KeyNotFoundException("One or more household deliveries were not found.");

            if (deliveries.Any(x => x.CampaignId != task.CampaignTeam.CampaignId))
                throw new InvalidOperationException("All deliveries must belong to the same campaign as the task.");

            var activeDeliveries = deliveries.Where(x => x.Status != HouseholdFulfillmentStatus.Delivered).ToList();
            if (activeDeliveries.Count == 0)
                throw new InvalidOperationException("All selected deliveries are already completed.");

            var title = !string.IsNullOrWhiteSpace(request.SubTaskTitle)
                ? request.SubTaskTitle!.Trim()
                : $"Nhóm giao hàng {activeDeliveries.Count} hộ";

            var note = !string.IsNullOrWhiteSpace(request.TaskNote)
                ? request.TaskNote!.Trim()
                : $"Subtask được tạo từ {activeDeliveries.Count} delivery thuộc campaign task {task.Title}.";

            var memberTask = new MemberTask
            {
                MemberTaskId = Guid.NewGuid(),
                CampaignTaskId = task.CampaignTaskId,
                VolunteerProfileId = volunteerProfile.VolunteerProfileId,
                SubTaskTitle = title,
                TaskNote = note,
                AssignedAt = DateTime.UtcNow,
                Status = MemberTaskStatus.Assigned
            };

            await _unitOfWork.MemberTasks.AddAsync(memberTask);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var delivery in activeDeliveries)
            {
                await _unitOfWork.MemberTaskDeliveries.AddAsync(new MemberTaskDelivery
                {
                    MemberTaskDeliveryId = Guid.NewGuid(),
                    MemberTaskId = memberTask.MemberTaskId,
                    HouseholdDeliveryId = delivery.HouseholdDeliveryId,
                    AssignedVolunteerProfileId = volunteerProfile.VolunteerProfileId,
                    Status = MemberTaskStatus.Assigned,
                    Note = $"Mapped from task generation for delivery {delivery.HouseholdDeliveryId}"
                });
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var savedTask = await _unitOfWork.MemberTasks.GetByIdWithDetailsAsync(memberTask.MemberTaskId, cancellationToken)
                ?? throw new KeyNotFoundException("Member task was not found after creation.");
            return [MapMemberTask(savedTask, ResolveVolunteerDisplayName(volunteerProfile))];
        }

        public async Task<List<MemberTaskResponse>> BulkAssignDeliveriesToMembersAsync(Guid campaignTaskId, BulkAssignDeliveriesToMembersRequest request, CancellationToken cancellationToken = default)
        {
            var task = await _unitOfWork.CampaignTasks.GetByIdWithDetailsAsync(campaignTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign task '{campaignTaskId}' was not found.");

            await GetReliefCampaignAsync(task.CampaignTeam.CampaignId, cancellationToken);
            await EnsureTeamLeaderOrCoordinatorAsync(task.CampaignTeam, cancellationToken);
            ValidateTaskEditable(task.Status);

            if (request.Assignments is null || request.Assignments.Count == 0)
                throw new InvalidOperationException("At least one assignment group is required.");

            var results = new List<MemberTaskResponse>();

            foreach (var assignment in request.Assignments)
            {
                var subTaskRequest = new CreateMemberTaskFromHouseholdsRequest
                {
                    VolunteerProfileId = assignment.VolunteerProfileId,
                    HouseholdDeliveryIds = assignment.HouseholdDeliveryIds,
                    SubTaskTitle = string.IsNullOrWhiteSpace(assignment.LineName)
                        ? assignment.SubTaskTitle
                        : string.IsNullOrWhiteSpace(assignment.SubTaskTitle)
                            ? assignment.LineName
                            : $"{assignment.LineName} - {assignment.SubTaskTitle}",
                    TaskNote = assignment.TaskNote
                };

                var created = await CreateMemberTasksFromHouseholdsAsync(campaignTaskId, subTaskRequest, cancellationToken);
                results.AddRange(created);
            }

            return results;
        }

        public async Task<List<MemberTaskDeliveryResponse>> AssignDeliveriesToMemberTaskAsync(Guid memberTaskId, AssignMemberTaskDeliveriesRequest request, CancellationToken cancellationToken = default)
        {
            var memberTask = await _unitOfWork.MemberTasks.GetByIdWithDetailsAsync(memberTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Member task '{memberTaskId}' was not found.");

            await EnsureTeamLeaderOrCoordinatorAsync(memberTask.CampaignTask.CampaignTeam, cancellationToken);
            ValidateTaskEditable(memberTask.CampaignTask.Status);

            var deliveries = await _unitOfWork.HouseholdDeliveries.GetByIdsAsync(request.HouseholdDeliveryIds, cancellationToken);
            if (deliveries.Count != request.HouseholdDeliveryIds.Distinct().Count())
                throw new KeyNotFoundException("One or more household deliveries were not found.");

            if (deliveries.Any(x => x.CampaignId != memberTask.CampaignTask.CampaignTeam.CampaignId))
                throw new InvalidOperationException("All deliveries must belong to the same campaign as the member task.");

            var existingLinks = await _unitOfWork.MemberTaskDeliveries.GetByMemberTaskIdAsync(memberTaskId, cancellationToken);
            var created = new List<MemberTaskDeliveryResponse>();

            foreach (var delivery in deliveries)
            {
                if (delivery.Status == HouseholdFulfillmentStatus.Delivered)
                    throw new InvalidOperationException($"Delivery '{delivery.HouseholdDeliveryId}' is already completed.");

                if (existingLinks.Any(x => x.HouseholdDeliveryId == delivery.HouseholdDeliveryId))
                    continue;

                var link = new MemberTaskDelivery
                {
                    MemberTaskDeliveryId = Guid.NewGuid(),
                    MemberTaskId = memberTaskId,
                    HouseholdDeliveryId = delivery.HouseholdDeliveryId,
                    AssignedVolunteerProfileId = request.AssignedVolunteerProfileId,
                    Status = MemberTaskStatus.Assigned,
                    Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim()
                };

                await _unitOfWork.MemberTaskDeliveries.AddAsync(link);
                created.Add(MapMemberTaskDelivery(link, delivery));
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return created;
        }

        public async Task<List<MemberTaskDeliveryResponse>> GetMemberTaskDeliveriesAsync(Guid memberTaskId, CancellationToken cancellationToken = default)
        {
            var memberTask = await _unitOfWork.MemberTasks.GetByIdWithDetailsAsync(memberTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Member task '{memberTaskId}' was not found.");

            await EnsureCanAccessMemberTaskAsync(memberTask, cancellationToken);

            return memberTask.MemberTaskDeliveries.Select(MapMemberTaskDelivery).ToList();
        }

        public async Task<MemberTaskDeliveryResponse> ChangeMemberTaskDeliveryStatusAsync(Guid memberTaskDeliveryId, ChangeMemberTaskDeliveryStatusRequest request, CancellationToken cancellationToken = default)
        {
            var link = await _unitOfWork.MemberTaskDeliveries.GetQueryable()
                .Include(x => x.MemberTask)
                    .ThenInclude(mt => mt.CampaignTask)
                        .ThenInclude(ct => ct.CampaignTeam)
                .Include(x => x.MemberTask)
                    .ThenInclude(mt => mt.CampaignTask)
                        .ThenInclude(ct => ct.MemberTasks)
                .Include(x => x.AssignedVolunteerProfile)
                .FirstOrDefaultAsync(x => x.MemberTaskDeliveryId == memberTaskDeliveryId, cancellationToken)
                ?? throw new KeyNotFoundException($"Member task delivery '{memberTaskDeliveryId}' was not found.");

            await EnsureCanOperateMemberTaskDeliveryAsync(link, cancellationToken);

            ValidateMemberTaskStatusTransition(link.Status, request.Status);
            link.Status = request.Status;
            link.Note = request.Note ?? link.Note;
            if (request.Status == MemberTaskStatus.Completed)
            {
                link.CompletedAt = DateTime.UtcNow;
                link.CompletedByUserId = _currentUser.UserId;
            }

            await _unitOfWork.MemberTaskDeliveries.UpdateAsync(link);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var saved = await _unitOfWork.MemberTaskDeliveries.GetQueryable()
                .FirstAsync(x => x.MemberTaskDeliveryId == memberTaskDeliveryId, cancellationToken);
            await SyncMemberTaskStatusFromDeliveriesAsync(saved.MemberTaskId, cancellationToken);
            return MapMemberTaskDelivery(saved);
        }

        public async Task<MemberTaskDeliveryResponse> CompleteMemberTaskDeliveryWithDeliveryAsync(Guid memberTaskDeliveryId, CompleteMemberTaskDeliveryWithDeliveryRequest request, CancellationToken cancellationToken = default)
        {
            var link = await _unitOfWork.MemberTaskDeliveries.GetQueryable()
                .Include(x => x.MemberTask)
                    .ThenInclude(mt => mt.CampaignTask)
                        .ThenInclude(ct => ct.CampaignTeam)
                .Include(x => x.MemberTask)
                    .ThenInclude(mt => mt.CampaignTask)
                        .ThenInclude(ct => ct.MemberTasks)
                .Include(x => x.AssignedVolunteerProfile)
                .FirstOrDefaultAsync(x => x.MemberTaskDeliveryId == memberTaskDeliveryId, cancellationToken)
                ?? throw new KeyNotFoundException($"Member task delivery '{memberTaskDeliveryId}' was not found.");

            await EnsureCanOperateMemberTaskDeliveryAsync(link, cancellationToken);

            var delivery = await _unitOfWork.HouseholdDeliveries.GetByIdWithProofsAsync(link.HouseholdDeliveryId, cancellationToken)
                ?? throw new KeyNotFoundException($"Household delivery '{link.HouseholdDeliveryId}' was not found.");

            if (delivery.Status == HouseholdFulfillmentStatus.Delivered)
                throw new InvalidOperationException("Delivery already completed.");

            delivery.Status = HouseholdFulfillmentStatus.Delivered;
            delivery.DeliveredAt = DateTime.UtcNow;
            delivery.DeliveredByUserId = _currentUser.UserId;
            if (!string.IsNullOrWhiteSpace(request.DeliveryNote))
                delivery.Notes = request.DeliveryNote.Trim();

            await _unitOfWork.HouseholdDeliveryProofs.AddAsync(new HouseholdDeliveryProof
            {
                HouseholdDeliveryProofId = Guid.NewGuid(),
                HouseholdDeliveryId = delivery.HouseholdDeliveryId,
                FileUrl = request.ProofFileUrl.Trim(),
                FileType = request.ProofContentType,
                Note = request.ProofNote,
                CapturedAt = DateTime.UtcNow,
                CapturedByUserId = _currentUser.UserId
            });

            await _unitOfWork.HouseholdDeliveries.UpdateAsync(delivery);

            var household = await _unitOfWork.CampaignHouseholds.GetByIdAsync(delivery.CampaignHouseholdId)
                ?? throw new KeyNotFoundException($"Campaign household '{delivery.CampaignHouseholdId}' was not found.");
            household.FulfillmentStatus = HouseholdFulfillmentStatus.Delivered;
            await _unitOfWork.CampaignHouseholds.UpdateAsync(household);

            link.Status = MemberTaskStatus.Completed;
            link.CompletedAt = DateTime.UtcNow;
            link.CompletedByUserId = _currentUser.UserId;
            if (!string.IsNullOrWhiteSpace(request.DeliveryNote))
                link.Note = request.DeliveryNote.Trim();

            await _unitOfWork.MemberTaskDeliveries.UpdateAsync(link);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await SyncMemberTaskStatusFromDeliveriesAsync(link.MemberTaskId, cancellationToken);

            var saved = await _unitOfWork.MemberTaskDeliveries.GetQueryable()
                .FirstAsync(x => x.MemberTaskDeliveryId == memberTaskDeliveryId, cancellationToken);
            return MapMemberTaskDelivery(saved);
        }

        public async Task<MemberTaskResponse> ChangeMemberTaskStatusAsync(Guid memberTaskId, ChangeMemberTaskStatusRequest request, CancellationToken cancellationToken = default)
        {
            var memberTask = await _unitOfWork.MemberTasks.GetByIdAsync(memberTaskId)
                ?? throw new KeyNotFoundException($"Member task '{memberTaskId}' was not found.");

            var task = await _unitOfWork.CampaignTasks.GetByIdWithDetailsAsync(memberTask.CampaignTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign task '{memberTask.CampaignTaskId}' was not found.");

            await GetReliefCampaignAsync(task.CampaignTeam.CampaignId, cancellationToken);
            ValidateMemberTaskStatusTransition(memberTask.Status, request.Status);

            var noteParts = SplitTaskNoteAndFailureReason(memberTask.TaskNote);
            memberTask.Status = request.Status;

            if (request.Status == MemberTaskStatus.Failed)
            {
                if (string.IsNullOrWhiteSpace(request.FailureReason))
                    throw new InvalidOperationException("Failure reason is required when marking a subtask as failed.");

                memberTask.TaskNote = ComposeTaskNoteWithFailureReason(noteParts.TaskNote, request.FailureReason);
                memberTask.CompletedAt = null;
            }
            else
            {
                memberTask.TaskNote = ComposeTaskNoteWithFailureReason(noteParts.TaskNote, null);

                if (request.Status == MemberTaskStatus.Completed)
                {
                    memberTask.CompletedAt = DateTime.UtcNow;
                }
                else
                {
                    memberTask.CompletedAt = null;
                }
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
            var anyActive = memberTasks.Any(mt => mt.Status is MemberTaskStatus.Assigned or MemberTaskStatus.InProgress);

            CampaignTaskStatus? newStatus = null;

            if (allTerminal && task.Status != CampaignTaskStatus.Completed)
            {
                // All subtasks done → auto-complete parent
                newStatus = CampaignTaskStatus.Completed;
            }
            else if (anyFailed && task.Status != CampaignTaskStatus.Blocked)
            {
                // Any failed subtask means the parent task is blocked until the leader/team resolves it.
                newStatus = CampaignTaskStatus.Blocked;
            }
            else if (!anyFailed && anyActive && task.Status != CampaignTaskStatus.InProgress)
            {
                // When failed subtasks are retried or work continues, reopen the parent task.
                newStatus = CampaignTaskStatus.InProgress;
            }
            if (newStatus.HasValue && newStatus.Value != task.Status)
            {
                task.Status = newStatus.Value;
                await _unitOfWork.CampaignTasks.UpdateAsync(task);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task SyncMemberTaskStatusFromDeliveriesAsync(Guid memberTaskId, CancellationToken cancellationToken)
        {
            var memberTask = await _unitOfWork.MemberTasks.GetByIdWithDetailsAsync(memberTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Member task '{memberTaskId}' was not found.");

            if (memberTask.MemberTaskDeliveries.Count == 0)
                return;

            if (memberTask.MemberTaskDeliveries.All(x => x.Status == MemberTaskStatus.Completed))
            {
                memberTask.Status = MemberTaskStatus.Completed;
                memberTask.CompletedAt = DateTime.UtcNow;
                await _unitOfWork.MemberTasks.UpdateAsync(memberTask);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await AutoUpdateParentTaskStatusAsync(memberTask.CampaignTask, cancellationToken);
            }
            else if (memberTask.MemberTaskDeliveries.Any(x => x.Status == MemberTaskStatus.InProgress) && memberTask.Status == MemberTaskStatus.Assigned)
            {
                memberTask.Status = MemberTaskStatus.InProgress;
                await _unitOfWork.MemberTasks.UpdateAsync(memberTask);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        private async Task EnsureCanAccessCampaignTaskAsync(CampaignTask task, CancellationToken cancellationToken)
        {
            await EnsureCanAccessMemberTaskTeamAsync(task.CampaignTeam, null, cancellationToken);
        }

        private async Task EnsureCanAccessMemberTaskAsync(MemberTask memberTask, CancellationToken cancellationToken)
        {
            await EnsureCanAccessMemberTaskTeamAsync(memberTask.CampaignTask.CampaignTeam, memberTask.VolunteerProfileId, cancellationToken);
        }

        private async Task EnsureTeamLeaderOrCoordinatorAsync(CampaignTeam campaignTeam, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var team = await _unitOfWork.Teams.GetByIdWithDetailsAsync(campaignTeam.TeamId)
                ?? throw new KeyNotFoundException($"Team '{campaignTeam.TeamId}' was not found.");

            if (team.LeaderId != currentUserId)
                throw new UnauthorizedAccessException("Only the team leader or coordinator can perform this action.");
        }

        private async Task EnsureCanOperateMemberTaskDeliveryAsync(MemberTaskDelivery link, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var team = await _unitOfWork.Teams.GetByIdWithDetailsAsync(link.MemberTask.CampaignTask.CampaignTeam.TeamId)
                ?? throw new KeyNotFoundException($"Team '{link.MemberTask.CampaignTask.CampaignTeam.TeamId}' was not found.");

            if (team.LeaderId == currentUserId)
                return;

            var volunteerProfile = await _unitOfWork.VolunteerProfiles.GetByUserIdAsync(currentUserId)
                ?? throw new KeyNotFoundException("Volunteer profile for current user was not found.");

            var assignedVolunteerId = link.AssignedVolunteerProfileId ?? link.MemberTask.VolunteerProfileId;
            if (assignedVolunteerId != volunteerProfile.VolunteerProfileId)
                throw new UnauthorizedAccessException("You are not allowed to update this assigned delivery.");
        }

        private async Task EnsureCanAccessMemberTaskTeamAsync(CampaignTeam campaignTeam, Guid? volunteerProfileId, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var team = await _unitOfWork.Teams.GetByIdWithDetailsAsync(campaignTeam.TeamId)
                ?? throw new KeyNotFoundException($"Team '{campaignTeam.TeamId}' was not found.");

            if (team.LeaderId == currentUserId)
                return;

            var moderatorProfile = await _unitOfWork.ModeratorProfiles.GetByUserIdAsync(currentUserId, cancellationToken);
            if (moderatorProfile?.ReliefStationId != null)
            {
                var belongsToModeratorStation = team.ReliefStationTeams.Any(rst =>
                    rst.ReliefStationId == moderatorProfile.ReliefStationId.Value);

                if (belongsToModeratorStation)
                    return;
            }

            if (volunteerProfileId.HasValue)
            {
                var volunteerProfile = await _unitOfWork.VolunteerProfiles.GetByUserIdAsync(currentUserId);
                if (volunteerProfile?.VolunteerProfileId == volunteerProfileId.Value)
                    return;
            }

            throw new UnauthorizedAccessException("You are not allowed to access this team task.");
        }

        public async Task DeleteAsync(Guid campaignTaskId, CancellationToken cancellationToken = default)
        {
            var task = await _unitOfWork.CampaignTasks.GetByIdWithDetailsAsync(campaignTaskId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign task '{campaignTaskId}' was not found.");

            await EnsureTeamLeaderOrCoordinatorAsync(task.CampaignTeam, cancellationToken);

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
                MemberTaskStatus.Failed => next is MemberTaskStatus.InProgress or MemberTaskStatus.Cancelled,
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
        {
            var noteParts = SplitTaskNoteAndFailureReason(memberTask.TaskNote);
            return new()
            {
                MemberTaskId = memberTask.MemberTaskId,
                CampaignTaskId = memberTask.CampaignTaskId,
                VolunteerProfileId = memberTask.VolunteerProfileId,
                VolunteerName = volunteerName ?? string.Empty,
                SubTaskTitle = memberTask.SubTaskTitle,
                TaskNote = noteParts.TaskNote,
                FailureReason = noteParts.FailureReason,
                AssignedAt = memberTask.AssignedAt,
                CompletedAt = memberTask.CompletedAt,
                Status = memberTask.Status,
                Deliveries = memberTask.MemberTaskDeliveries.Select(MapMemberTaskDelivery).ToList()
            };
        }

        private static (string? TaskNote, string? FailureReason) SplitTaskNoteAndFailureReason(string? storedNote)
        {
            if (string.IsNullOrWhiteSpace(storedNote))
                return (null, null);

            var trimmed = storedNote.Trim();
            var markerIndex = trimmed.IndexOf(FailureReasonMarker, StringComparison.Ordinal);

            if (markerIndex < 0)
                return (trimmed, null);

            var taskNote = trimmed[..markerIndex].Trim();
            var failureReason = trimmed[(markerIndex + FailureReasonMarker.Length)..].Trim();

            return (
                string.IsNullOrWhiteSpace(taskNote) ? null : taskNote,
                string.IsNullOrWhiteSpace(failureReason) ? null : failureReason
            );
        }

        private static string? ComposeTaskNoteWithFailureReason(string? taskNote, string? failureReason)
        {
            var normalizedTaskNote = string.IsNullOrWhiteSpace(taskNote) ? null : taskNote.Trim();
            var normalizedFailureReason = string.IsNullOrWhiteSpace(failureReason) ? null : failureReason.Trim();

            if (normalizedFailureReason is null)
                return normalizedTaskNote;

            return string.IsNullOrWhiteSpace(normalizedTaskNote)
                ? $"[FAILURE_REASON] {normalizedFailureReason}"
                : $"{normalizedTaskNote}{FailureReasonMarker}{normalizedFailureReason}";
        }

        private static MemberTaskDeliveryResponse MapMemberTaskDelivery(MemberTaskDelivery item)
            => MapMemberTaskDelivery(item, item.HouseholdDelivery);

        private static MemberTaskDeliveryResponse MapMemberTaskDelivery(MemberTaskDelivery item, HouseholdDelivery delivery)
            => new()
            {
                MemberTaskDeliveryId = item.MemberTaskDeliveryId,
                MemberTaskId = item.MemberTaskId,
                HouseholdDeliveryId = item.HouseholdDeliveryId,
                CampaignHouseholdId = delivery.CampaignHouseholdId,
                HouseholdCode = delivery.CampaignHousehold?.HouseholdCode ?? string.Empty,
                HeadOfHouseholdName = delivery.CampaignHousehold?.HeadOfHouseholdName ?? string.Empty,
                Address = delivery.CampaignHousehold?.Address,
                AssignedVolunteerProfileId = item.AssignedVolunteerProfileId,
                AssignedVolunteerName = ResolveVolunteerDisplayName(item.AssignedVolunteerProfile),
                Status = item.Status,
                DeliveryStatus = delivery.Status,
                ScheduledAt = delivery.ScheduledAt,
                CompletedAt = item.CompletedAt,
                Note = item.Note
            };

        private static string ResolveVolunteerDisplayName(VolunteerProfile? volunteerProfile)
            => volunteerProfile?.User?.DisplayName
                ?? volunteerProfile?.User?.UserName
                ?? volunteerProfile?.User?.Email
                ?? string.Empty;
    }
}
