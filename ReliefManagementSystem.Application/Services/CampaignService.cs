using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests;
using ReliefManagementSystem.Application.Features.Campaign.Dtos.Responses;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CampaignService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<CampaignResponse> CreateAsync(CreateCampaignRequest request, CancellationToken cancellationToken = default)
        {
            if (request.EndDate < request.StartDate)
            {
                throw new InvalidOperationException("EndDate phải lớn hơn hoặc bằng StartDate.");
            }

            if (!await _unitOfWork.Locations.ExistsAsync(request.LocationId))
            {
                throw new KeyNotFoundException($"Location '{request.LocationId}' was not found.");
            }

            ValidateGoalDuplicates(request.Goals);

            if (request.Type == CampaignType.Fundraising && request.Goals.Count == 0)
            {
                throw new InvalidOperationException("Campaign Fundraising phải có ít nhất 1 mục tiêu tài nguyên.");
            }

            var creatorId = _currentUserService.UserId ?? Guid.Empty;

            var campaign = new Domain.Entities.Campaign
            {
                CampaignId = Guid.NewGuid(),
                LocationId = request.LocationId,
                CreatedBy = creatorId,
                Name = request.Name.Trim(),
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AreaRadiusKm = request.AreaRadiusKm,
                AddressDetail = request.AddressDetail,
                Type = request.Type,
                Status = CampaignStatus.Draft,
                AllowOverTarget = request.AllowOverTarget,
                BudgetTotal = 0,
                BudgetSpent = 0
            };

            await _unitOfWork.Campaigns.AddAsync(campaign);

            foreach (var goalReq in request.Goals)
            {
                var target = goalReq.TargetAmount;
                if (goalReq.ResourceType == CampaignResourceType.People)
                {
                    // People target = số người còn thiếu = max(0, target - available)
                    target = Math.Max(0, goalReq.TargetAmount - request.AvailablePeopleCount);
                }

                var goal = new CampaignResourceGoal
                {
                    CampaignResourceGoalId = Guid.NewGuid(),
                    CampaignId = campaign.CampaignId,
                    ResourceType = goalReq.ResourceType,
                    TargetAmount = target,
                    ReceivedAmount = 0,
                    IsMet = target == 0,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Campaigns.AddGoalAsync(goal, cancellationToken);
            }

            if (request.ReliefStationId.HasValue)
            {
                await AttachStationInternalAsync(campaign.CampaignId, request.ReliefStationId.Value, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var saved = await _unitOfWork.Campaigns.GetWithStationsAsync(campaign.CampaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaign.CampaignId}' was not found after creation.");

            return await BuildCampaignResponseAsync(saved, cancellationToken);
        }

        public async Task<CampaignResponse> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithStationsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            return await BuildCampaignResponseAsync(campaign, cancellationToken);
        }

        public async Task<Pagination<CampaignSummaryResponse>> GetPagedAsync(CampaignListQueryRequest request, CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _unitOfWork.Campaigns.GetPagedAsync(
                request.PageIndex,
                request.PageSize,
                request.Keyword,
                request.Status,
                request.Type,
                request.LocationId,
                cancellationToken);

            var mapped = items.Select(MapSummary).ToList();
            return new Pagination<CampaignSummaryResponse>(mapped, totalCount, request.PageIndex, request.PageSize);
        }

        public async Task<CampaignResponse> ChangeStatusAsync(Guid campaignId, ChangeCampaignStatusRequest request, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithStationsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            ValidateStatusTransition(campaign, request.Status);

            campaign.Status = request.Status;
            await _unitOfWork.Campaigns.UpdateAsync(campaign);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await BuildCampaignResponseAsync(campaign, cancellationToken);
        }

        public async Task<CampaignResponse> AttachStationAsync(Guid campaignId, AttachCampaignStationRequest request, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithStationsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            await AttachStationInternalAsync(campaignId, request.ReliefStationId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updated = await _unitOfWork.Campaigns.GetWithStationsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            return await BuildCampaignResponseAsync(updated, cancellationToken);
        }

        public async Task UpdateProgressAsync(Guid campaignId, CampaignResourceType resourceType, decimal amountDelta, CancellationToken cancellationToken = default)
        {
            if (amountDelta == 0) return;

            var campaign = await _unitOfWork.Campaigns.GetWithGoalsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            var goal = campaign.ResourceGoals.FirstOrDefault(g => g.ResourceType == resourceType)
                ?? throw new InvalidOperationException($"Campaign '{campaignId}' không có mục tiêu '{resourceType}'.");

            var next = goal.ReceivedAmount + amountDelta;
            if (next < 0) next = 0;

            if (!campaign.AllowOverTarget && goal.TargetAmount > 0 && next > goal.TargetAmount)
            {
                next = goal.TargetAmount;
            }

            goal.ReceivedAmount = next;
            goal.IsMet = goal.TargetAmount <= 0 || goal.ReceivedAmount >= goal.TargetAmount;
            goal.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Campaigns.UpdateGoalAsync(goal, cancellationToken);

            // Auto-close fundraising campaign when all active goals met.
            if (campaign.Type == CampaignType.Fundraising && campaign.Status == CampaignStatus.Active)
            {
                var allMet = campaign.ResourceGoals.Count != 0 && campaign.ResourceGoals.All(g => g.IsMet);
                if (allMet)
                {
                    campaign.Status = CampaignStatus.GoalsMet;
                    await _unitOfWork.Campaigns.UpdateAsync(campaign);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task AttachStationInternalAsync(Guid campaignId, Guid reliefStationId, CancellationToken cancellationToken)
        {
            if (!await _unitOfWork.ReliefStations.ExistsAsync(reliefStationId))
            {
                throw new KeyNotFoundException($"Relief station '{reliefStationId}' was not found.");
            }

            // Campaign chỉ được gắn tối đa 1 station.
            var hasActive = await _unitOfWork.Campaigns.HasAnyActiveStationAsync(campaignId, cancellationToken);
            if (hasActive)
            {
                throw new InvalidOperationException("Campaign đã có station đang active. Mỗi campaign chỉ gắn tối đa 1 station.");
            }

            var already = await _unitOfWork.Campaigns.IsStationAlreadyAttachedAsync(campaignId, reliefStationId, cancellationToken);
            if (already)
            {
                throw new InvalidOperationException("Station này đã được gắn vào campaign.");
            }

            await _unitOfWork.Campaigns.AddStationAsync(new CampaignStation
            {
                CampaignId = campaignId,
                ReliefStationId = reliefStationId,
                IsActive = true,
                AssignedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        private static void ValidateGoalDuplicates(List<CampaignGoalRequest> goals)
        {
            var duplicated = goals
                .GroupBy(g => g.ResourceType)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();

            if (duplicated.Count > 0)
            {
                throw new InvalidOperationException($"Mỗi loại mục tiêu chỉ được khai báo 1 lần: {string.Join(", ", duplicated)}.");
            }
        }

        private static void ValidateStatusTransition(Domain.Entities.Campaign campaign, CampaignStatus next)
        {
            if (campaign.Status == next) return;

            bool valid = campaign.Type switch
            {
                CampaignType.Fundraising => (campaign.Status, next) switch
                {
                    (CampaignStatus.Draft, CampaignStatus.Active) => true,
                    (CampaignStatus.Active, CampaignStatus.Suspended) => true,
                    (CampaignStatus.Active, CampaignStatus.GoalsMet) => true,
                    (CampaignStatus.Active, CampaignStatus.Cancelled) => true,
                    (CampaignStatus.Suspended, CampaignStatus.Active) => true,
                    (CampaignStatus.Suspended, CampaignStatus.Cancelled) => true,
                    (CampaignStatus.GoalsMet, CampaignStatus.Completed) => true,
                    _ => false
                },

                CampaignType.Relief => (campaign.Status, next) switch
                {
                    (CampaignStatus.Draft, CampaignStatus.ReadyToExecute) => true,
                    (CampaignStatus.ReadyToExecute, CampaignStatus.InProgress) => true,
                    (CampaignStatus.InProgress, CampaignStatus.Suspended) => true,
                    (CampaignStatus.InProgress, CampaignStatus.Completed) => true,
                    (CampaignStatus.InProgress, CampaignStatus.Cancelled) => true,
                    (CampaignStatus.Suspended, CampaignStatus.InProgress) => true,
                    (CampaignStatus.Suspended, CampaignStatus.Cancelled) => true,
                    _ => false
                },

                CampaignType.Rescue => (campaign.Status, next) switch
                {
                    (CampaignStatus.Draft, CampaignStatus.Active) => true,
                    (CampaignStatus.Active, CampaignStatus.Closing) => true,
                    (CampaignStatus.Closing, CampaignStatus.Completed) => true,
                    (CampaignStatus.Active, CampaignStatus.Cancelled) => true,
                    (CampaignStatus.Closing, CampaignStatus.Cancelled) => true,
                    _ => false
                },

                _ => false
            };

            if (!valid)
            {
                throw new InvalidOperationException(
                    $"Không thể chuyển trạng thái campaign từ '{campaign.Status}' sang '{next}' cho loại '{campaign.Type}'.");
            }
        }

        private async Task<CampaignResponse> BuildCampaignResponseAsync(Domain.Entities.Campaign campaign, CancellationToken cancellationToken)
        {
            // Ensure goals loaded for response
            var goals = campaign.ResourceGoals.Any()
                ? campaign.ResourceGoals.ToList()
                : await _unitOfWork.Campaigns.GetGoalsAsync(campaign.CampaignId, cancellationToken);

            var stations = campaign.CampaignStations
                .Select(cs => new CampaignStationResponse
                {
                    ReliefStationId = cs.ReliefStationId,
                    ReliefStationName = cs.ReliefStation?.Name ?? string.Empty,
                    IsActive = cs.IsActive,
                    AssignedAt = cs.AssignedAt
                })
                .ToList();

            return new CampaignResponse
            {
                CampaignId = campaign.CampaignId,
                LocationId = campaign.LocationId,
                CreatedBy = campaign.CreatedBy,
                Name = campaign.Name,
                Description = campaign.Description,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                Latitude = campaign.Latitude,
                Longitude = campaign.Longitude,
                AreaRadiusKm = campaign.AreaRadiusKm,
                AddressDetail = campaign.AddressDetail,
                Status = campaign.Status,
                Type = campaign.Type,
                AllowOverTarget = campaign.AllowOverTarget,
                CreatedAt = campaign.CreatedAt,
                Goals = goals.Select(MapGoal).ToList(),
                Stations = stations
            };
        }

        private static CampaignSummaryResponse MapSummary(Domain.Entities.Campaign campaign)
        {
            decimal overall;
            if (!campaign.ResourceGoals.Any())
            {
                overall = 0;
            }
            else
            {
                var percents = campaign.ResourceGoals
                    .Select(g => g.TargetAmount <= 0 ? 100m : Math.Min(100m, (g.ReceivedAmount / g.TargetAmount) * 100m))
                    .ToList();

                overall = percents.Average();
            }

            return new CampaignSummaryResponse
            {
                CampaignId = campaign.CampaignId,
                Name = campaign.Name,
                Status = campaign.Status,
                Type = campaign.Type,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                AllowOverTarget = campaign.AllowOverTarget,
                OverallProgressPercent = Math.Round(overall, 2)
            };
        }

        private static CampaignGoalResponse MapGoal(CampaignResourceGoal goal)
        {
            var progress = goal.TargetAmount <= 0
                ? 100m
                : (goal.ReceivedAmount / goal.TargetAmount) * 100m;

            return new CampaignGoalResponse
            {
                CampaignResourceGoalId = goal.CampaignResourceGoalId,
                ResourceType = goal.ResourceType,
                TargetAmount = goal.TargetAmount,
                ReceivedAmount = goal.ReceivedAmount,
                IsMet = goal.IsMet,
                ProgressPercent = Math.Round(progress, 2)
            };
        }
    }
}
