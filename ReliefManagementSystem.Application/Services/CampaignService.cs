using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests;
using ReliefManagementSystem.Application.Features.Campaign.Dtos.Responses;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System.Text.Json;

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

            if (request.Type == CampaignType.Fundraising)
            {
                if (request.Goals.Count == 0)
                {
                    throw new InvalidOperationException("Campaign Fundraising phải có ít nhất 1 mục tiêu Money hoặc People.");
                }

                if (request.Goals.Any(g => g.ResourceType == CampaignResourceType.Supplies))
                {
                    throw new InvalidOperationException("Campaign Fundraising không được dùng mục tiêu Supplies ở phase hiện tại.");
                }

                if (!request.Goals.Any(g => g.ResourceType is CampaignResourceType.Money or CampaignResourceType.People))
                {
                    throw new InvalidOperationException("Campaign Fundraising phải có ít nhất 1 mục tiêu Money hoặc People.");
                }
            }

            var creatorId = _currentUserService.UserId ?? Guid.Empty;
            var availablePeopleCount = await _unitOfWork.Teams.GetAvailablePeopleCountAsync(cancellationToken);

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
                CompletionRule = request.CompletionRule,
                Status = CampaignStatus.Draft,
                AllowOverTarget = request.AllowOverTarget,
                BudgetTotal = 0,
                BudgetSpent = 0
            };

            await _unitOfWork.Campaigns.AddAsync(campaign);

            foreach (var goalReq in request.Goals)
            {
                var target = goalReq.TargetAmount;
                if (goalReq.ResourceType == CampaignResourceType.People && request.Type != CampaignType.Fundraising)
                {
                    // People target = số người còn thiếu = max(0, target - available team members in system)
                    target = Math.Max(0, goalReq.TargetAmount - availablePeopleCount);
                }

                var goal = new CampaignResourceGoal
                {
                    CampaignResourceGoalId = Guid.NewGuid(),
                    CampaignId = campaign.CampaignId,
                    ResourceType = goalReq.ResourceType,
                    TargetAmount = target,
                    ReceivedAmount = 0,
                    IsRequired = goalReq.IsRequired,
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

        public async Task<CampaignResponse> UpdateAsync(Guid campaignId, UpdateCampaignRequest request, CancellationToken cancellationToken = default)
        {
            if (request.EndDate < request.StartDate)
            {
                throw new InvalidOperationException("EndDate phải lớn hơn hoặc bằng StartDate.");
            }

            var campaign = await _unitOfWork.Campaigns.GetWithDetailsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            if (!CanEditCampaign(campaign))
            {
                throw new InvalidOperationException(GetEditabilityErrorMessage(campaign));
            }

            campaign.Name = request.Name.Trim();
            campaign.Description = request.Description;
            campaign.StartDate = request.StartDate;
            campaign.EndDate = request.EndDate;
            campaign.Latitude = request.Latitude;
            campaign.Longitude = request.Longitude;
            campaign.AreaRadiusKm = request.AreaRadiusKm;
            campaign.AddressDetail = request.AddressDetail;
            campaign.AllowOverTarget = request.AllowOverTarget;
            campaign.CompletionRule = request.CompletionRule;

            await _unitOfWork.Campaigns.UpdateAsync(campaign);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await BuildCampaignResponseAsync(campaign, cancellationToken);
        }

        public async Task<CampaignResponse> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithStationsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            return await BuildCampaignResponseAsync(campaign, cancellationToken);
        }

        public async Task<CampaignInventoryBalanceResponse> GetInventoryBalanceAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithStationsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            var campaignInventory = await _unitOfWork.CampaignInventories.GetByCampaignIdWithDetailsAsync(campaignId, cancellationToken);
            var stocks = campaignInventory?.Stocks
                .Where(x => x.CurrentQuantity > 0)
                .OrderBy(x => x.SupplyItem.Name)
                .ToList() ?? [];

            return new CampaignInventoryBalanceResponse
            {
                CampaignId = campaign.CampaignId,
                CampaignInventoryId = campaignInventory?.CampaignInventoryId,
                BudgetTotal = campaign.BudgetTotal,
                BudgetSpent = campaign.BudgetSpent,
                RemainingBudget = campaign.BudgetTotal - campaign.BudgetSpent,
                DistinctSupplyItemCount = stocks.Count,
                TotalQuantity = stocks.Sum(x => x.CurrentQuantity),
                Items = stocks.Select(x => new CampaignInventoryBalanceItemResponse
                {
                    SupplyItemId = x.SupplyItemId,
                    SupplyItemName = x.SupplyItem?.Name ?? string.Empty,
                    SupplyItemUnit = x.SupplyItem?.Unit ?? string.Empty,
                    Quantity = x.CurrentQuantity
                }).ToList()
            };
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
                request.ForVolunteerRegistration,
                request.SupportsVolunteerRegistration,
                request.HasMoneyGoal,
                request.SupportsDonation,
                cancellationToken);

            var mapped = items.Select(MapSummary).ToList();
            return new Pagination<CampaignSummaryResponse>(mapped, totalCount, request.PageIndex, request.PageSize);
        }

        public async Task<PublicCampaignSummaryResponse> GetPublicSummaryAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithDetailsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            var goals = campaign.ResourceGoals.Any()
                ? campaign.ResourceGoals.ToList()
                : await _unitOfWork.Campaigns.GetGoalsAsync(campaignId, cancellationToken);

            var procurementOrders = campaign.Type == CampaignType.Fundraising
                ? []
                : await _unitOfWork.ProcurementOrders.GetByCampaignAsync(campaignId, cancellationToken);

            var supplyAllocations = campaign.Type == CampaignType.Fundraising
                ? []
                : await _unitOfWork.SupplyAllocations.GetByCampaignIdAsync(campaignId, cancellationToken);

            var peopleGoal = goals.FirstOrDefault(g => g.ResourceType == CampaignResourceType.People);

            return new PublicCampaignSummaryResponse
            {
                CampaignId = campaign.CampaignId,
                Name = campaign.Name,
                Description = campaign.Description,
                Type = campaign.Type,
                Status = campaign.Status,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                TotalMoneyReceived = campaign.BudgetTotal,
                TotalMoneySpent = campaign.BudgetSpent,
                RemainingBudget = campaign.BudgetTotal - campaign.BudgetSpent,
                PeopleTarget = peopleGoal?.TargetAmount ?? 0,
                PeopleReached = peopleGoal?.ReceivedAmount ?? 0,
                ProcurementOrderCount = procurementOrders.Count,
                ProcurementReceivedCount = procurementOrders.Count(p => p.Status == ProcurementStatus.Received),
                ProcurementEstimatedTotal = procurementOrders.Sum(p => p.TotalEstimatedCost),
                ProcurementActualTotal = procurementOrders.Sum(p => p.TotalActualCost ?? 0),
                TotalSuppliesPurchasedUnits = procurementOrders.Sum(p => p.Items.Sum(i => i.ReceivedQuantity ?? 0)),
                TotalSuppliesAllocatedUnits = supplyAllocations.Sum(a => a.Items.Sum(i => i.Quantity)),
                Goals = goals.Select(MapGoal).ToList()
            };
        }

        public async Task<CampaignResponse> ChangeStatusAsync(Guid campaignId, ChangeCampaignStatusRequest request, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithDetailsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            await ValidateReliefReadinessAsync(campaign, request.Status, cancellationToken);
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

            if (campaign.Type == CampaignType.Fundraising)
            {
                throw new InvalidOperationException("Fundraising campaign không gắn relief station.");
            }

            await AttachStationInternalAsync(campaignId, request.ReliefStationId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updated = await _unitOfWork.Campaigns.GetWithStationsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            return await BuildCampaignResponseAsync(updated, cancellationToken);
        }

        public async Task<CampaignResponse> DetachStationAsync(Guid campaignId, Guid reliefStationId, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithDetailsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            if (campaign.Type == CampaignType.Fundraising)
            {
                throw new InvalidOperationException("Fundraising campaign không dùng relief station.");
            }

            var station = await _unitOfWork.Campaigns.GetStationAsync(campaignId, reliefStationId, cancellationToken)
                ?? throw new KeyNotFoundException($"Station '{reliefStationId}' is not attached to campaign '{campaignId}'.");

            station.IsActive = false;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await BuildCampaignResponseAsync(campaign, cancellationToken);
        }

        public async Task<CampaignBudgetTransferResponse> ExtractBudgetAsync(Guid fundraisingCampaignId, ExtractCampaignBudgetRequest request, CancellationToken cancellationToken = default)
        {
            if (request.Amount <= 0)
                throw new InvalidOperationException("Extract amount must be greater than zero.");

            var source = await _unitOfWork.Campaigns.GetWithDetailsAsync(fundraisingCampaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{fundraisingCampaignId}' was not found.");

            var target = await _unitOfWork.Campaigns.GetWithDetailsAsync(request.TargetReliefCampaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{request.TargetReliefCampaignId}' was not found.");

            if (source.Type != CampaignType.Fundraising)
                throw new InvalidOperationException("Source campaign must be a fundraising campaign.");

            if (target.Type != CampaignType.Relief)
                throw new InvalidOperationException("Target campaign must be a relief campaign.");

            var sourceRemaining = source.BudgetTotal - source.BudgetSpent;
            if (request.Amount > sourceRemaining)
            {
                await LogFinancialFailureAsync(
                    "CampaignBudgetTransfer",
                    "ExtractFailed",
                    fundraisingCampaignId.ToString(),
                    new
                    {
                        fundraisingCampaignId,
                        request.TargetReliefCampaignId,
                        request.Amount,
                        sourceRemaining,
                        Message = "Insufficient fundraising campaign balance for extraction."
                    },
                    cancellationToken);

                throw new InvalidOperationException("Extract amount exceeds the fundraising campaign balance.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                source.BudgetSpent += request.Amount;
                target.BudgetTotal += request.Amount;

                var transfer = new CampaignBudgetTransfer
                {
                    CampaignBudgetTransferId = Guid.NewGuid(),
                    SourceCampaignId = source.CampaignId,
                    TargetCampaignId = target.CampaignId,
                    Amount = request.Amount,
                    TransferredByUserId = _currentUserService.UserId,
                    TransferredAt = DateTime.UtcNow,
                    Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim()
                };

                await _unitOfWork.CampaignBudgetTransfers.AddAsync(transfer);
                await _unitOfWork.Campaigns.UpdateAsync(source);
                await _unitOfWork.Campaigns.UpdateAsync(target);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return new CampaignBudgetTransferResponse
                {
                    CampaignBudgetTransferId = transfer.CampaignBudgetTransferId,
                    SourceCampaignId = transfer.SourceCampaignId,
                    TargetCampaignId = transfer.TargetCampaignId,
                    Amount = transfer.Amount,
                    TransferredByUserId = transfer.TransferredByUserId,
                    TransferredAt = transfer.TransferredAt,
                    Note = transfer.Note,
                    SourceRemainingBudget = source.BudgetTotal - source.BudgetSpent,
                    TargetRemainingBudget = target.BudgetTotal - target.BudgetSpent
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                await LogFinancialFailureAsync(
                    "CampaignBudgetTransfer",
                    "ExtractFailed",
                    fundraisingCampaignId.ToString(),
                    new
                    {
                        fundraisingCampaignId,
                        request.TargetReliefCampaignId,
                        request.Amount,
                        request.Note,
                        Exception = ex.Message
                    },
                    cancellationToken);
                throw;
            }
        }

        public async Task<CampaignTeamResponse> AssignTeamAsync(Guid campaignId, AssignCampaignTeamRequest request, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithGoalsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            var team = await _unitOfWork.Teams.GetByIdWithDetailsAsync(request.TeamId)
                ?? throw new KeyNotFoundException($"Team '{request.TeamId}' was not found.");

            if (campaign.Type == CampaignType.Rescue && team.TeamType != TeamType.Rescue)
            {
                throw new InvalidOperationException("Chiến dịch cứu hộ chỉ nhận team cứu hộ.");
            }

            if (campaign.Type != CampaignType.Rescue && team.TeamType != TeamType.Relief)
            {
                throw new InvalidOperationException("Team cứu hộ chỉ được tham gia chiến dịch cứu hộ.");
            }

            var existing = await _unitOfWork.Campaigns.GetCampaignTeamAsync(campaignId, request.TeamId, cancellationToken);
            if (existing != null)
            {
                throw new InvalidOperationException("Team này đã được gán vào campaign.");
            }

            var campaignTeam = new CampaignTeam
            {
                CampaignTeamId = Guid.NewGuid(),
                CampaignId = campaignId,
                TeamId = request.TeamId,
                Role = request.Role,
                Status = request.InitialStatus,
                AssignedAt = DateTime.UtcNow,
                IsDelete = false
            };

            await _unitOfWork.Campaigns.AddCampaignTeamAsync(campaignTeam, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var memberCount = await _unitOfWork.Teams.GetAvailablePeopleCountByTeamAsync(request.TeamId, cancellationToken);
            if (campaign.Type != CampaignType.Fundraising &&
                request.InitialStatus is CampaignTeamStatus.Accepted or CampaignTeamStatus.Active)
            {
                await UpdateProgressAsync(campaignId, CampaignResourceType.People, memberCount, cancellationToken);
            }

            return new CampaignTeamResponse
            {
                CampaignTeamId = campaignTeam.CampaignTeamId,
                CampaignId = campaignId,
                TeamId = request.TeamId,
                TeamName = team.Name,
                Role = campaignTeam.Role,
                Status = campaignTeam.Status,
                AssignedAt = campaignTeam.AssignedAt,
                MemberCount = memberCount
            };
        }

        public async Task<IReadOnlyList<CampaignTeamResponse>> GetTeamsAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            if (!await _unitOfWork.Campaigns.ExistsAsync(campaignId))
            {
                throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");
            }

            var items = await _unitOfWork.Campaigns.GetCampaignTeamsAsync(campaignId, cancellationToken);
            var results = new List<CampaignTeamResponse>();
            foreach (var item in items)
            {
                var memberCount = await _unitOfWork.Teams.GetTeamMemberCountAsync(item.TeamId, cancellationToken);
                results.Add(new CampaignTeamResponse
                {
                    CampaignTeamId = item.CampaignTeamId,
                    CampaignId = item.CampaignId,
                    TeamId = item.TeamId,
                    TeamName = item.Team?.Name ?? string.Empty,
                    Role = item.Role,
                    Status = item.Status,
                    AssignedAt = item.AssignedAt,
                    MemberCount = memberCount
                });
            }

            return results;
        }

        public async Task<CampaignTeamResponse> UpdateTeamStatusAsync(Guid campaignId, Guid teamId, UpdateCampaignTeamStatusRequest request, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithGoalsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            var campaignTeam = await _unitOfWork.Campaigns.GetCampaignTeamAsync(campaignId, teamId, cancellationToken)
                ?? throw new KeyNotFoundException($"Team '{teamId}' is not assigned to campaign '{campaignId}'.");

            var previousStatus = campaignTeam.Status;
            if (previousStatus == request.Status)
            {
                var sameCount = await _unitOfWork.Teams.GetAvailablePeopleCountByTeamAsync(teamId, cancellationToken);
                return new CampaignTeamResponse
                {
                    CampaignTeamId = campaignTeam.CampaignTeamId,
                    CampaignId = campaignId,
                    TeamId = teamId,
                    TeamName = campaignTeam.Team?.Name ?? string.Empty,
                    Role = campaignTeam.Role,
                    Status = campaignTeam.Status,
                    AssignedAt = campaignTeam.AssignedAt,
                    MemberCount = sameCount
                };
            }

            campaignTeam.Status = request.Status;
            await _unitOfWork.Campaigns.UpdateCampaignTeamAsync(campaignTeam, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var memberCount = await _unitOfWork.Teams.GetAvailablePeopleCountByTeamAsync(teamId, cancellationToken);
            var wasCounted = previousStatus is CampaignTeamStatus.Accepted or CampaignTeamStatus.Active;
            var isCounted = request.Status is CampaignTeamStatus.Accepted or CampaignTeamStatus.Active;

            var shouldTrackPeopleByTeam = campaign.Type != CampaignType.Fundraising;

            if (shouldTrackPeopleByTeam && !wasCounted && isCounted)
            {
                await UpdateProgressAsync(campaignId, CampaignResourceType.People, memberCount, cancellationToken);
            }
            else if (shouldTrackPeopleByTeam && wasCounted && !isCounted)
            {
                await UpdateProgressAsync(campaignId, CampaignResourceType.People, -memberCount, cancellationToken);
            }

            return new CampaignTeamResponse
            {
                CampaignTeamId = campaignTeam.CampaignTeamId,
                CampaignId = campaignId,
                TeamId = teamId,
                TeamName = campaignTeam.Team?.Name ?? string.Empty,
                Role = campaignTeam.Role,
                Status = campaignTeam.Status,
                AssignedAt = campaignTeam.AssignedAt,
                MemberCount = memberCount
            };
        }

        public async Task RemoveTeamAsync(Guid campaignId, Guid teamId, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithGoalsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            var campaignTeam = await _unitOfWork.Campaigns.GetCampaignTeamAsync(campaignId, teamId, cancellationToken)
                ?? throw new KeyNotFoundException($"Team '{teamId}' is not assigned to campaign '{campaignId}'.");

            var memberCount = await _unitOfWork.Teams.GetAvailablePeopleCountByTeamAsync(teamId, cancellationToken);
            if (campaign.Type != CampaignType.Fundraising &&
                campaignTeam.Status is CampaignTeamStatus.Accepted or CampaignTeamStatus.Active)
            {
                await UpdateProgressAsync(campaignId, CampaignResourceType.People, -memberCount, cancellationToken);
            }

            campaignTeam.IsDelete = true;
            campaignTeam.Status = CampaignTeamStatus.Cancelled;
            await _unitOfWork.Campaigns.UpdateCampaignTeamAsync(campaignTeam, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<CampaignAssignedVehicleResponse> AssignVehicleToTeamAsync(Guid campaignId, AssignCampaignVehicleRequest request, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithDetailsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            if (campaign.Type != CampaignType.Relief)
                throw new InvalidOperationException("Vehicle assignment is only available for relief campaigns.");

            var campaignTeam = await _unitOfWork.Campaigns.GetCampaignTeamsAsync(campaignId, cancellationToken);
            var matchedTeam = campaignTeam.FirstOrDefault(x => x.CampaignTeamId == request.CampaignTeamId)
                ?? throw new KeyNotFoundException($"Campaign team '{request.CampaignTeamId}' was not found in campaign.");

            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId)
                ?? throw new KeyNotFoundException($"Vehicle '{request.VehicleId}' was not found.");

            var assignment = new CampaignVehicle
            {
                CampaignVehicleId = Guid.NewGuid(),
                CampaignId = campaignId,
                CampaignTeamId = request.CampaignTeamId,
                VehicleId = request.VehicleId,
                AssignedDriverId = request.AssignedDriverId,
                StartDate = request.StartDate ?? DateTime.UtcNow,
                EndDate = request.EndDate,
                Status = request.Status,
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim()
            };

            await _unitOfWork.CampaignVehicles.AddAsync(assignment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CampaignAssignedVehicleResponse
            {
                CampaignVehicleId = assignment.CampaignVehicleId,
                VehicleId = assignment.VehicleId,
                LicensePlate = vehicle.LicensePlate,
                VehicleTypeName = vehicle.VehicleType?.TypeName ?? string.Empty,
                CampaignTeamId = assignment.CampaignTeamId,
                AssignedDriverId = assignment.AssignedDriverId,
                Status = assignment.Status,
                StartDate = assignment.StartDate,
                EndDate = assignment.EndDate,
                Note = assignment.Note
            };
        }

        public async Task<IReadOnlyList<CampaignAssignedVehicleResponse>> GetCampaignVehiclesAsync(Guid campaignId, Guid? campaignTeamId, CancellationToken cancellationToken = default)
        {
            if (!await _unitOfWork.Campaigns.ExistsAsync(campaignId))
                throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            var assignments = await _unitOfWork.CampaignVehicles.GetAllAsync();
            var vehicles = await _unitOfWork.Vehicles.GetAllAsync();

            var filtered = assignments.Where(x => x.CampaignId == campaignId);
            if (campaignTeamId.HasValue)
                filtered = filtered.Where(x => x.CampaignTeamId == campaignTeamId.Value);

            return filtered.Select(x =>
            {
                var vehicle = vehicles.FirstOrDefault(v => v.VehicleId == x.VehicleId);
                return new CampaignAssignedVehicleResponse
                {
                    CampaignVehicleId = x.CampaignVehicleId,
                    VehicleId = x.VehicleId,
                    LicensePlate = vehicle?.LicensePlate ?? string.Empty,
                    VehicleTypeName = vehicle?.VehicleType?.TypeName ?? string.Empty,
                    CampaignTeamId = x.CampaignTeamId,
                    AssignedDriverId = x.AssignedDriverId,
                    Status = x.Status,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Note = x.Note
                };
            }).ToList();
        }

        public async Task<CampaignAssignedVehicleResponse> UpdateCampaignVehicleAssignmentAsync(Guid campaignId, Guid campaignVehicleId, UpdateCampaignVehicleAssignmentRequest request, CancellationToken cancellationToken = default)
        {
            var assignment = await _unitOfWork.CampaignVehicles.GetByIdAsync(campaignVehicleId)
                ?? throw new KeyNotFoundException($"Campaign vehicle assignment '{campaignVehicleId}' was not found.");

            if (assignment.CampaignId != campaignId)
                throw new InvalidOperationException("Campaign vehicle assignment does not belong to campaign.");

            if (request.CampaignTeamId.HasValue)
                assignment.CampaignTeamId = request.CampaignTeamId.Value;
            if (request.AssignedDriverId.HasValue)
                assignment.AssignedDriverId = request.AssignedDriverId.Value;
            if (request.StartDate.HasValue)
                assignment.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue)
                assignment.EndDate = request.EndDate.Value;
            if (request.Status.HasValue)
                assignment.Status = request.Status.Value;
            if (request.Note is not null)
                assignment.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

            await _unitOfWork.CampaignVehicles.UpdateAsync(assignment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(assignment.VehicleId);

            return new CampaignAssignedVehicleResponse
            {
                CampaignVehicleId = assignment.CampaignVehicleId,
                VehicleId = assignment.VehicleId,
                LicensePlate = vehicle?.LicensePlate ?? string.Empty,
                VehicleTypeName = vehicle?.VehicleType?.TypeName ?? string.Empty,
                CampaignTeamId = assignment.CampaignTeamId,
                AssignedDriverId = assignment.AssignedDriverId,
                Status = assignment.Status,
                StartDate = assignment.StartDate,
                EndDate = assignment.EndDate,
                Note = assignment.Note
            };
        }

        public async Task RemoveCampaignVehicleAssignmentAsync(Guid campaignId, Guid campaignVehicleId, CancellationToken cancellationToken = default)
        {
            var assignment = await _unitOfWork.CampaignVehicles.GetByIdAsync(campaignVehicleId)
                ?? throw new KeyNotFoundException($"Campaign vehicle assignment '{campaignVehicleId}' was not found.");

            if (assignment.CampaignId != campaignId)
                throw new InvalidOperationException("Campaign vehicle assignment does not belong to campaign.");

            await _unitOfWork.CampaignVehicles.DeleteAsync(assignment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<CampaignVolunteerRegistrationResponse> RegisterVolunteerAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithGoalsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            if (campaign.Type != CampaignType.Fundraising)
            {
                throw new InvalidOperationException("Chỉ fundraising campaign mới cho phép đăng ký volunteer theo campaign.");
            }

            if (campaign.Status != CampaignStatus.Active)
            {
                throw new InvalidOperationException("Chỉ fundraising campaign đang Active mới cho phép đăng ký volunteer.");
            }

            var peopleGoal = campaign.ResourceGoals.FirstOrDefault(g => g.ResourceType == CampaignResourceType.People);
            if (peopleGoal is null)
            {
                throw new InvalidOperationException("Campaign này không có mục tiêu People để đăng ký volunteer.");
            }

            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

            var volunteerProfile = await _unitOfWork.VolunteerProfiles.GetByUserIdAsync(userId);

            if (volunteerProfile is null)
            {
                throw new InvalidOperationException("Bạn cần tạo volunteer profile trước khi đăng ký campaign fundraising.");
            }

            if (volunteerProfile.VerificationStatus != VerificationStatus.Approved || volunteerProfile.Status != VolunteerStatus.Active)
            {
                throw new InvalidOperationException("Volunteer profile phải được approved và active trước khi đăng ký campaign fundraising.");
            }

            var user = volunteerProfile.User
                ?? await _unitOfWork.Users.GetUserById(userId);

            var existing = await _unitOfWork.CampaignVolunteerRegistrations.GetActiveAsync(campaignId, userId, cancellationToken);
            if (existing != null)
            {
                throw new InvalidOperationException("Bạn đã đăng ký volunteer cho campaign này rồi.");
            }

            await EnsureVolunteerRegistrationCapacityAsync(campaignId, cancellationToken);

            var registration = new CampaignVolunteerRegistration
            {
                CampaignVolunteerRegistrationId = Guid.NewGuid(),
                CampaignId = campaignId,
                UserId = userId,
                Status = CampaignVolunteerRegistrationStatus.Registered,
                RegisteredAt = DateTime.UtcNow,
                User = user
            };

            await _unitOfWork.CampaignVolunteerRegistrations.AddAsync(registration, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await UpdateProgressAsync(campaignId, CampaignResourceType.People, 1, cancellationToken);

            return MapVolunteerRegistration(registration);
        }

        public async Task CancelVolunteerRegistrationAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithGoalsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            if (campaign.Type != CampaignType.Fundraising)
            {
                throw new InvalidOperationException("Chỉ fundraising campaign mới có đăng ký volunteer theo campaign.");
            }

            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");
            var registration = await _unitOfWork.CampaignVolunteerRegistrations.GetActiveAsync(campaignId, userId, cancellationToken)
                ?? throw new KeyNotFoundException("Không tìm thấy đăng ký volunteer active cho campaign này.");

            registration.Status = CampaignVolunteerRegistrationStatus.Cancelled;
            registration.CancelledAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await UpdateProgressAsync(campaignId, CampaignResourceType.People, -1, cancellationToken);
        }

        public async Task<IReadOnlyList<CampaignVolunteerRegistrationResponse>> GetVolunteerRegistrationsAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithGoalsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            if (campaign.Type != CampaignType.Fundraising)
            {
                throw new InvalidOperationException("Chỉ fundraising campaign mới có danh sách đăng ký volunteer theo campaign.");
            }

            var registrations = await _unitOfWork.CampaignVolunteerRegistrations.GetByCampaignAsync(campaignId, cancellationToken);
            return registrations.Select(MapVolunteerRegistration).ToList();
        }

        public async Task EnsureVolunteerRegistrationCapacityAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithGoalsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            var peopleGoal = campaign.ResourceGoals.FirstOrDefault(g => g.ResourceType == CampaignResourceType.People)
                ?? throw new InvalidOperationException("Campaign này không có mục tiêu People để đăng ký volunteer.");

            if (!campaign.AllowOverTarget && peopleGoal.TargetAmount > 0 && peopleGoal.ReceivedAmount >= peopleGoal.TargetAmount)
            {
                throw new InvalidOperationException("Campaign đã đủ chỉ tiêu tình nguyện viên, không cho phép đăng ký vượt chỉ tiêu.");
            }
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

            // Auto-close fundraising campaign according to completion rule.
            if (campaign.Type == CampaignType.Fundraising && campaign.Status == CampaignStatus.Active)
            {
                if (ShouldMarkGoalsMet(campaign))
                {
                    campaign.Status = CampaignStatus.Completed;
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

            if (campaign.Type == CampaignType.Fundraising && next == CampaignStatus.GoalsMet && !ShouldMarkGoalsMet(campaign))
            {
                throw new InvalidOperationException("Campaign chưa đạt điều kiện để chuyển sang GoalsMet.");
            }

            if (campaign.Type == CampaignType.Fundraising && next == CampaignStatus.Completed && campaign.Status == CampaignStatus.Active)
            {
                return;
            }

            bool valid = GetAllowedNextStatuses(campaign).Contains(next);

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
                CompletionRule = campaign.CompletionRule,
                AllowOverTarget = campaign.AllowOverTarget,
                AllowedNextStatuses = GetAllowedNextStatuses(campaign),
                CreatedAt = campaign.CreatedAt,
                Goals = goals.Select(MapGoal).ToList(),
                Stations = stations
            };
        }

        private static bool CanEditCampaign(Domain.Entities.Campaign campaign)
            => GetEditableStatuses(campaign.Type).Contains(campaign.Status);

        private async Task LogFinancialFailureAsync(string entityName, string action, string primaryKey, object payload, CancellationToken cancellationToken)
        {
            await _unitOfWork.AuditLogs.AddAsync(new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                EntityName = entityName,
                Action = action,
                Timestamp = DateTime.UtcNow,
                UserId = _currentUserService.UserId,
                PrimaryKey = primaryKey,
                NewValues = JsonSerializer.Serialize(payload)
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static string GetEditabilityErrorMessage(Domain.Entities.Campaign campaign)
            => campaign.Type switch
            {
                CampaignType.Relief => "Chỉ có thể cập nhật relief campaign ở trạng thái Draft, Active hoặc Suspended.",
                CampaignType.Fundraising => "Chỉ có thể cập nhật fundraising campaign ở trạng thái Draft, Active hoặc Suspended.",
                CampaignType.Rescue => "Chỉ có thể cập nhật rescue campaign ở trạng thái Draft hoặc Active.",
                _ => $"Không thể cập nhật campaign loại '{campaign.Type}' ở trạng thái '{campaign.Status}'."
            };

        private static List<CampaignStatus> GetEditableStatuses(CampaignType campaignType)
            => campaignType switch
            {
                CampaignType.Fundraising => [CampaignStatus.Draft, CampaignStatus.Active, CampaignStatus.Suspended],
                CampaignType.Relief => [CampaignStatus.Draft, CampaignStatus.Active, CampaignStatus.Suspended],
                CampaignType.Rescue => [CampaignStatus.Draft, CampaignStatus.Active],
                _ => []
            };

        private static List<CampaignStatus> GetAllowedNextStatuses(Domain.Entities.Campaign campaign)
            => GetAllowedNextStatuses(campaign.Type, campaign.Status, ShouldMarkGoalsMet(campaign));

        private static List<CampaignStatus> GetAllowedNextStatuses(
            CampaignType campaignType,
            CampaignStatus current,
            bool goalsMet)
            => campaignType switch
            {
                CampaignType.Fundraising => current switch
                {
                    CampaignStatus.Draft => [CampaignStatus.Active],
                    CampaignStatus.Active => goalsMet
                        ? [CampaignStatus.Suspended, CampaignStatus.GoalsMet, CampaignStatus.Completed, CampaignStatus.Cancelled]
                        : [CampaignStatus.Suspended, CampaignStatus.Completed, CampaignStatus.Cancelled],
                    CampaignStatus.Suspended => [CampaignStatus.Active, CampaignStatus.Completed, CampaignStatus.Cancelled],
                    CampaignStatus.GoalsMet => [CampaignStatus.Completed],
                    _ => []
                },
                CampaignType.Relief => current switch
                {
                    CampaignStatus.Draft => [CampaignStatus.Active],
                    CampaignStatus.Active => [CampaignStatus.Suspended, CampaignStatus.Completed, CampaignStatus.Cancelled],
                    CampaignStatus.Suspended => [CampaignStatus.Active, CampaignStatus.Cancelled],
                    _ => []
                },
                CampaignType.Rescue => current switch
                {
                    CampaignStatus.Draft => [CampaignStatus.Active],
                    CampaignStatus.Active => [CampaignStatus.Closing, CampaignStatus.Cancelled],
                    CampaignStatus.Closing => [CampaignStatus.Completed, CampaignStatus.Cancelled],
                    _ => []
                },
                _ => []
            };

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
                CompletionRule = campaign.CompletionRule,
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
                IsRequired = goal.IsRequired,
                IsMet = goal.IsMet,
                ProgressPercent = Math.Round(progress, 2)
            };
        }

        private static CampaignVolunteerRegistrationResponse MapVolunteerRegistration(CampaignVolunteerRegistration registration)
        {
            return new CampaignVolunteerRegistrationResponse
            {
                CampaignVolunteerRegistrationId = registration.CampaignVolunteerRegistrationId,
                CampaignId = registration.CampaignId,
                UserId = registration.UserId,
                UserDisplayName = registration.User?.DisplayName ?? registration.User?.UserName ?? registration.User?.Email ?? string.Empty,
                UserEmail = registration.User?.Email,
                Status = registration.Status,
                RegisteredAt = registration.RegisteredAt,
                CancelledAt = registration.CancelledAt
            };
        }

        private async Task ValidateReliefReadinessAsync(Domain.Entities.Campaign campaign, CampaignStatus next, CancellationToken cancellationToken)
        {
            if (campaign.Type != CampaignType.Relief)
            {
                return;
            }

            var station = campaign.CampaignStations.FirstOrDefault(s => s.IsActive);
            var activeTeams = campaign.CampaignTeams.Where(t => !t.IsDelete && (t.Status == CampaignTeamStatus.Accepted || t.Status == CampaignTeamStatus.Active)).ToList();

            if (next == CampaignStatus.Active)
            {
                if (activeTeams.Count == 0)
                {
                    throw new InvalidOperationException("Relief campaign cần ít nhất 1 team Accepted/Active trước khi chuyển sang Active.");
                }

                if (station is null)
                {
                    throw new InvalidOperationException("Relief campaign cần gắn 1 relief station active trước khi chuyển sang Active.");
                }

                var inventory = await _unitOfWork.Inventories.GetActiveByReliefStationAsync(station.ReliefStationId, cancellationToken);
                if (inventory is null)
                {
                    throw new InvalidOperationException("Relief station của campaign chưa có inventory active để vận hành.");
                }

                var hasStock = inventory.InventoryItems.Any(i => i.CurrentQuantity > 0);
                var allocations = await _unitOfWork.SupplyAllocations.GetByCampaignIdAsync(campaign.CampaignId, cancellationToken);
                var hasUsableAllocation = allocations.Any(a => a.Status == SupplyAllocationStatus.Pending || a.Status == SupplyAllocationStatus.Approved || a.Status == SupplyAllocationStatus.Delivered);

                if (!hasStock && !hasUsableAllocation && campaign.BudgetTotal <= campaign.BudgetSpent)
                {
                    throw new InvalidOperationException("Relief campaign cần có nguồn lực khả dụng (stock, allocation hoặc budget còn lại) trước khi chuyển sang Active.");
                }
            }

            if (next == CampaignStatus.Completed)
            {
                // Old relief-flow checks (DistributionSessions / ReliefRequests) removed.
                // Add new completion-readiness checks here when the replacement flow is ready.
            }
        }

        private static bool ShouldMarkGoalsMet(Domain.Entities.Campaign campaign)
        {
            if (!campaign.ResourceGoals.Any())
            {
                return false;
            }

            return campaign.CompletionRule switch
            {
                CampaignCompletionRule.AllGoalsMet => campaign.ResourceGoals.All(g => g.IsMet),
                CampaignCompletionRule.RequiredGoalsMet =>
                    campaign.ResourceGoals.Where(g => g.IsRequired).Any()
                    && campaign.ResourceGoals.Where(g => g.IsRequired).All(g => g.IsMet),
                CampaignCompletionRule.ManualOnly => false,
                _ => false
            };
        }
    }
}
