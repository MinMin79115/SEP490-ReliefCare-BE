using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request;
using ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Response;
using ReliefManagementSystem.Application.Features.Notification;
using ReliefManagementSystem.Application.Features.InventoryTransaction.DTOs.Request;
using ReliefManagementSystem.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ReliefManagementSystem.Application.Services
{
    public class RescueRequestService : IRescueRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IGoongDistanceService _goongDistanceService;
        private readonly IGoongRouteService _goongRouteService;
        private readonly IWeatherService _weatherService;
        private readonly INotificationService _notificationService;
        private readonly IInventoryTransactionService _inventoryTransactionService;

        public RescueRequestService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IGoongDistanceService goongDistanceService,
            IGoongRouteService goongRouteService,
            IWeatherService weatherService,
            INotificationService notificationService,
            IInventoryTransactionService inventoryTransactionService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _goongDistanceService = goongDistanceService;
            _goongRouteService = goongRouteService;
            _weatherService = weatherService;
            _notificationService = notificationService;
            _inventoryTransactionService = inventoryTransactionService;
        }

        public async Task<DistanceMatrixProbeResponse> ProbeDistanceMatrixAsync(
            double originLat,
            double originLng,
            List<double> destinationLats,
            List<double> destinationLngs,
            CancellationToken cancellationToken = default)
        {
            destinationLats ??= new List<double>();
            destinationLngs ??= new List<double>();

            var pairCount = Math.Min(destinationLats.Count, destinationLngs.Count);
            var destinations = new List<(double lat, double lng)>(pairCount);

            for (var i = 0; i < pairCount; i++)
            {
                destinations.Add((destinationLats[i], destinationLngs[i]));
            }

            var matrixResult = await _goongDistanceService.GetDistanceMatrixAsync(
                originLat,
                originLng,
                destinations,
                cancellationToken: cancellationToken);

            var response = new DistanceMatrixProbeResponse
            {
                OriginLat = originLat,
                OriginLng = originLng
            };

            for (var i = 0; i < pairCount; i++)
            {
                var element = i < matrixResult.Elements.Count ? matrixResult.Elements[i] : null;
                response.Items.Add(new DistanceMatrixProbeItem
                {
                    DestinationLat = destinations[i].lat,
                    DestinationLng = destinations[i].lng,
                    Status = element?.Status ?? string.Empty,
                    DistanceMeters = element?.DistanceMeters,
                    DurationSeconds = element?.DurationSeconds
                });
            }

            return response;
        }

        /// <summary>Gửi yêu cầu cứu hộ mới</summary>
        public async Task<RescueRequestResponseDto> CreateRescueRequestAsync(
            CreateRescueRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var createdAt = DateTime.UtcNow;

            // 1. Lấy thông tin người dùng hiện tại (nếu có)
            var currentUserId = _currentUserService.UserId;
            Domain.Entities.ApplicationUser? currentUser = null;

            if (currentUserId.HasValue)
            {
                currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId.Value);
            }

            // 1.1 Xác định CampaignId hợp lệ cho rescue request
            var attachedCampaignId = await ResolveCampaignIdForRescueRequestAsync(createdAt, cancellationToken);

            // 2. Tạo RescueRequest entity
            var rescueRequest = new Domain.Entities.RescueRequest
            {
                RequestId = Guid.NewGuid(),
                RequestType = Domain.Enum.RequestType.Rescue,
                DisasterType = (Domain.Enum.DisasterType)request.DisasterType,
                RescueRequestType = request.RescueType,
                Description = request.Description,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Accuracy = request.Accuracy,
                Address = request.Address ?? string.Empty,
                CampaignId = attachedCampaignId,
                Note = request.Note,
                ReporterUserId = currentUserId, // can be null for anonymous reports
                ReporterFullName = currentUser?.UserName ?? request.ReporterFullName ?? "Anonymous",
                ReporterPhone = currentUser?.PhoneNumber ?? request.ReporterPhone ?? string.Empty,
                CreatedAt = createdAt,
                RescueRequestStatus = Domain.Enum.RescueRequestStatus.Pending,
                DispatchMode = Domain.Enum.DispatchMode.NearestStation,
                PriorityPoint = null
            };

            // 3. Thêm attachments
            if (request.Attachments != null && request.Attachments.Count > 0)
            {
                foreach (var attachment in request.Attachments)
                {
                    rescueRequest.Attachments.Add(new Attachment
                    {
                        AttachmentId = Guid.NewGuid(),
                        RequestId = rescueRequest.RequestId,
                        FileUrl = attachment.FileUrl,
                        ContentType = attachment.ContentType,
                        AttachmentType = AttachmentType.RequestEvidence,
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }

            var verificationStatus = RequestVerificationStatus.Pending;
            var verificationMethod = VerificationMethod.None;
            string? verificationNote = null;

            if (request.RescueType == RescueRequestType.Emergency || request.RescueType == RescueRequestType.Normal)
            {
                if (request.RescueType == RescueRequestType.Emergency)
                {
                    verificationMethod = VerificationMethod.SystemAutoCheck;
                }

                try
                {
                    var weather = await _weatherService.GetCurrentWeatherAsync(
                        request.Latitude,
                        request.Longitude,
                        cancellationToken);

                    rescueRequest.WeatherCondition = weather.Condition;
                    rescueRequest.WeatherTempC = weather.TemperatureC;
                    rescueRequest.WeatherWindKph = weather.WindKph;
                    rescueRequest.WeatherPrecipMm = weather.PrecipMm;
                    rescueRequest.WeatherVisibilityKm = weather.VisibilityKm;
                    rescueRequest.WeatherRiskScore = weather.WeatherRiskScore;
                    rescueRequest.WeatherRiskLevel = weather.WeatherRiskLevel;
                    rescueRequest.WeatherObservedAt = weather.ObservedAt;

                    verificationNote =
                        $"Weather: Condition={weather.Condition}, TempC={weather.TemperatureC:0.##}, WindKph={weather.WindKph:0.##}, PrecipMm={weather.PrecipMm:0.##}, VisibilityKm={weather.VisibilityKm:0.##}, RiskScore={weather.WeatherRiskScore}, RiskLevel={weather.WeatherRiskLevel}";

                    if (request.RescueType == RescueRequestType.Emergency)
                    {
                        verificationStatus = weather.WeatherRiskScore >= 40
                            ? RequestVerificationStatus.Approved
                            : RequestVerificationStatus.Pending;
                        rescueRequest.PriorityPoint = 85;
                        rescueRequest.RescuePriorityLevel = RescuePriorityLevel.Critical;
                    }
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    if (request.RescueType == RescueRequestType.Emergency)
                    {
                        verificationStatus = RequestVerificationStatus.Pending;
                        verificationMethod = VerificationMethod.SystemAutoCheck;
                    }

                    var errorDetail = ex switch
                    {
                        HttpRequestException httpEx when httpEx.StatusCode.HasValue
                            => $"HTTP {(int)httpEx.StatusCode.Value} ({httpEx.StatusCode.Value}): {httpEx.Message}",
                        HttpRequestException httpEx
                            => $"HTTP request error: {httpEx.Message}",
                        TaskCanceledException tce
                            => $"Timeout/Canceled: {tce.Message}",
                        _ => ex.Message
                    };

                    verificationNote = $"Weather lookup failed; pending manual verification. Detail: {errorDetail}";
                    rescueRequest.WeatherCondition = "Unknown";
                    rescueRequest.WeatherRiskLevel = "Unknown";
                }
            }

            var verification = new RequestVerification
            {
                RequestVerificationId = Guid.NewGuid(),
                RequestId = rescueRequest.RequestId,
                Status = verificationStatus,
                Method = verificationMethod,
                Note = verificationNote
            };

            rescueRequest.Verifications.Add(verification);

            // Validate business rule before persist:
            // Normal request must provide selected priority criteria.
            if (request.RescueType == RescueRequestType.Normal &&
                (request.SelectedPriorityCriteriaIds == null || request.SelectedPriorityCriteriaIds.Count == 0))
            {
                throw new InvalidOperationException("Normal rescue request requires selectedPriorityCriteriaIds.");
            }


            // 4. Lưu RescueRequest vào database (before priorities so RescueRequestId exists)
            await _unitOfWork.RescueRequests.AddAsync(rescueRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. Nếu user gửi Normal request và có SelectedPriorityCriteriaIds -> tính điểm từ các mục user chọn
            if (request.RescueType == RescueRequestType.Normal)
            {
                // Load priority criteria for the disaster type and filter by selected ids
                var allCriteriaForDisaster = await _unitOfWork.PriorityCriterias.GetByDisasterTypeAsync(
                    rescueRequest.DisasterType, cancellationToken);

                var selectedCriteria = allCriteriaForDisaster
                    .Where(p => request.SelectedPriorityCriteriaIds.Contains(p.PriorityCriteriaId))
                    .ToList();

                int totalPoints = 0;

                foreach (var crit in selectedCriteria)
                {
                    totalPoints += crit.Point;

                    var rp = new RescueRequestPriority
                    {
                        RescueRequestId = rescueRequest.RequestId,
                        PriorityCriteriaId = crit.PriorityCriteriaId,
                        AppliedPoint = crit.Point,
                        Status = "SelectedByUser"
                    };

                    await _unitOfWork.RescueRequestPriorities.AddAsync(rp);
                }

                // Determine level and persist
                var priorityLevel = CalculatePriorityLevel(totalPoints);
                rescueRequest.PriorityPoint = totalPoints;
                rescueRequest.RescuePriorityLevel = priorityLevel;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // 6. Dispatch station ngay khi tạo request (Normal/Emergency)
            var selectedStation = await SelectStationForRescueRequestAsync(
                rescueRequest.Latitude,
                rescueRequest.Longitude,
                cancellationToken);

            var routeSnapshot = await BuildRouteSnapshotAsync(
                selectedStation.Latitude,
                selectedStation.Longitude,
                rescueRequest.Latitude,
                rescueRequest.Longitude,
                cancellationToken);

            rescueRequest.StationToRequestDistanceKm = routeSnapshot.DistanceKm;
            rescueRequest.StationToRequestDurationMinutes = routeSnapshot.DurationMinutes;
            rescueRequest.StationToRequestDistanceMeters = routeSnapshot.DistanceMeters;
            rescueRequest.StationToRequestDurationSeconds = routeSnapshot.DurationSeconds;

            var operation = new RescueOperation
            {
                RescueOperationId = Guid.NewGuid(),
                RescueRequestId = rescueRequest.RequestId,
                ReliefStationId = selectedStation.ReliefStationId,
                TeamId = null,
                Status = RescueOperationStatus.Pending,
                StartedAt = DateTime.UtcNow
            };

            await _unitOfWork.RescueOperations.AddAsync(operation);

            rescueRequest.DispatchMode = DispatchMode.NearestStation;
            if (request.RescueType == RescueRequestType.Emergency)
            {
                rescueRequest.RescueRequestStatus = verification.Status == RequestVerificationStatus.Approved && operation.ReliefStationId.HasValue
                    ? RescueRequestStatus.Verified
                    : RescueRequestStatus.Pending;
            }
            else
            {
                rescueRequest.RescueRequestStatus = RescueRequestStatus.Pending;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (operation.ReliefStationId.HasValue)
            {
                await NotifyModeratorsOnRescueRequestCreatedAsync(
                    operation.ReliefStationId.Value,
                    rescueRequest,
                    cancellationToken);
            }

            // 8. Trả về response
            return await GetRescueRequestByIdAsync(rescueRequest.RequestId, cancellationToken);
        }

        public async Task<RescueRequestResponseDto> VerifyRescueRequestAsync(
            Guid requestId,
            VerifyRescueRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            var request = await _unitOfWork.RescueRequests.GetByIdAsync(requestId, cancellationToken);
            if (request == null)
                throw new InvalidOperationException($"Rescue request {requestId} not found");

            var pendingVerification = request.Verifications
                .Where(v => v.Status == RequestVerificationStatus.Pending)
                .OrderByDescending(v => v.RequestVerificationId)
                .FirstOrDefault();

            if (pendingVerification == null)
                throw new InvalidOperationException("No pending verification found for this rescue request.");

            pendingVerification.Status = dto.Status;
            pendingVerification.Method = dto.Method;
            pendingVerification.Note = dto.Note;
            pendingVerification.Reason = dto.Reason;
            pendingVerification.VerifiedBy = _currentUserService.UserId;
            pendingVerification.VerifiedAt = DateTime.UtcNow;

            if (dto.Status == RequestVerificationStatus.Approved)
            {
                request.RescueRequestStatus = RescueRequestStatus.Verified;
            }
            else if (dto.Status == RequestVerificationStatus.Rejected)
            {
                request.RescueRequestStatus = RescueRequestStatus.Cancelled;
            }

            request.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await NotifyReporterOnRescueRequestVerifiedAsync(request, dto.Status, cancellationToken);

            return await GetRescueRequestByIdAsync(requestId, cancellationToken);
        }

        public async Task<RescueRequestResponseDto> AssignTeamToRescueAsync(
            Guid requestId,
            AssignRescueTeamRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            await EnsureRescueTeamTypeAsync(dto.TeamId, cancellationToken);

            var activeBatch = await _unitOfWork.RescueBatches.GetActiveByTeamIdAsync(dto.TeamId, cancellationToken);
            var currentBatchVehicleIds = activeBatch?.Items
                .Where(i => i.Status == RescueBatchItemStatus.Pending || i.Status == RescueBatchItemStatus.InProgress)
                .SelectMany(i => i.RescueRequest?.RescueOperations ?? Enumerable.Empty<RescueOperation>())
                .Where(o => o.TeamId == dto.TeamId)
                .OrderByDescending(o => o.StartedAt)
                .SelectMany(o => GetAssignedVehicles(o).Select(v => v.VehicleId))
                .Distinct()
                .ToList() ?? new List<Guid>();

            var requestedVehicleIds = (dto.VehicleIds ?? new List<Guid>())
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (requestedVehicleIds.Count == 0 && dto.VehicleId.HasValue)
            {
                requestedVehicleIds.Add(dto.VehicleId.Value);
            }

            var effectiveVehicleIds = requestedVehicleIds;
            if (currentBatchVehicleIds.Count > 0)
            {
                if (requestedVehicleIds.Count > 0 && !requestedVehicleIds.OrderBy(x => x).SequenceEqual(currentBatchVehicleIds.OrderBy(x => x)))
                {
                    throw new InvalidOperationException("Team đang dùng bộ xe khác trong active batch. Không thể đổi xe khi batch chưa hoàn tất.");
                }

                effectiveVehicleIds = currentBatchVehicleIds;
            }

            var request = await _unitOfWork.RescueRequests.GetByIdAsync(requestId, cancellationToken);
            if (request == null)
                throw new InvalidOperationException($"Rescue request {requestId} not found");

            var stationOperation = request.RescueOperations
                .Where(o => o.ReliefStationId.HasValue && o.Status != RescueOperationStatus.Cancelled)
                .OrderByDescending(o => o.StartedAt)
                .FirstOrDefault();

            if (stationOperation == null)
                throw new InvalidOperationException("No dispatched station operation found for this rescue request.");

            var stationTeamAssignment = await _unitOfWork.ReliefStationTeams.GetByStationAndTeamAsync(
                stationOperation.ReliefStationId!.Value,
                dto.TeamId,
                cancellationToken);

            if (stationTeamAssignment == null || stationTeamAssignment.Status != ReliefTeamAssignmentStatus.Approved)
                throw new InvalidOperationException("Team does not belong to dispatched station or assignment is not approved.");

            var assignedVehicles = new List<Vehicle>();
            foreach (var vehicleId in effectiveVehicleIds)
            {
                var assignedVehicle = await _unitOfWork.Vehicles.GetByIdWithDetailsAsync(vehicleId);
                if (assignedVehicle == null || assignedVehicle.IsDeleted)
                    throw new InvalidOperationException("Vehicle not found.");

                if (!assignedVehicle.ReliefStationId.HasValue || assignedVehicle.ReliefStationId.Value != stationOperation.ReliefStationId!.Value)
                    throw new InvalidOperationException("Vehicle does not belong to dispatched station.");

                var reusingBatchVehicle = currentBatchVehicleIds.Contains(assignedVehicle.VehicleId);

                if (assignedVehicle.Status != VehicleStatus.Free && !reusingBatchVehicle)
                    throw new InvalidOperationException("Vehicle is not available.");

                if (assignedVehicle.TeamId.HasValue && assignedVehicle.TeamId.Value != dto.TeamId)
                    throw new InvalidOperationException("Vehicle is assigned to another team.");

                assignedVehicles.Add(assignedVehicle);
            }

            stationOperation.TeamId = dto.TeamId;
            stationOperation.VehicleId = effectiveVehicleIds.FirstOrDefault();
            stationOperation.Status = RescueOperationStatus.Assigned;
            stationOperation.Note = dto.Note;

            foreach (var assignedVehicle in assignedVehicles)
            {
                assignedVehicle.Status = VehicleStatus.Busy;
            }

            await ReplaceRescueOperationVehiclesAsync(stationOperation, effectiveVehicleIds, dto.Note, cancellationToken);
            await SyncRescueOperationSuppliesAsync(stationOperation, dto.Supplies, dto.Note, cancellationToken);

            request.RescueRequestStatus = RescueRequestStatus.Assigned;
            request.UpdatedAt = DateTime.UtcNow;

            await EnsureActiveBatchAndAppendRequestAsync(dto.TeamId, request.RequestId, cancellationToken);
            await RecalculateBatchEtaFromLatestTrackingAsync(dto.TeamId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await NotifyOnRescueRequestAssignedAsync(request, dto.TeamId, cancellationToken);

            return await GetRescueRequestByIdAsync(requestId, cancellationToken);
        }

        public async Task<DispatchPreviewResponseDto> PreviewSmartAssignAsync(
            Guid requestId,
            DispatchPreviewRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            await EnsureRescueTeamTypeAsync(dto.TeamId, cancellationToken);

            return await BuildSmartAssignPreviewAsync(requestId, dto.TeamId, dto.AllowPreempt,
                dto.NormalNearRouteThresholdKm, dto.EmergencyNearRouteThresholdKm, cancellationToken);
        }

        public async Task<RescueBatchQueueResponseDto> SmartAssignTeamToRescueAsync(
            Guid requestId,
            SmartAssignRescueTeamRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            await EnsureRescueTeamTypeAsync(dto.TeamId, cancellationToken);

            var request = await _unitOfWork.RescueRequests.GetByIdAsync(requestId, cancellationToken)
                ?? throw new InvalidOperationException($"Rescue request {requestId} not found");
            var allActiveBatches = await _unitOfWork.RescueBatches.GetAllActiveWithItemsAsync(cancellationToken);
            var eligibility = EvaluateDispatchEligibility(request, dto.TeamId, allActiveBatches);
            if (!eligibility.CanDispatch)
            {
                throw new InvalidOperationException(eligibility.BlockReason ?? "Request cannot be dispatched.");
            }

            var preview = await BuildSmartAssignPreviewAsync(
                requestId,
                dto.TeamId,
                dto.AllowPreempt,
                dto.NormalNearRouteThresholdKm,
                dto.EmergencyNearRouteThresholdKm,
                cancellationToken);

            if (!preview.Eligible)
            {
                throw new InvalidOperationException(string.Join("; ", preview.Reasons));
            }

            await AssignTeamToRescueAsync(requestId, new AssignRescueTeamRequestDto
            {
                TeamId = dto.TeamId,
                VehicleId = dto.VehicleId,
                VehicleIds = dto.VehicleIds,
                Note = dto.Note
            }, cancellationToken);

            var activeBatch = await _unitOfWork.RescueBatches.GetActiveByTeamIdAsync(dto.TeamId, cancellationToken)
                ?? throw new InvalidOperationException("No active rescue batch found for team after assign.");

            var itemsByRequestId = activeBatch.Items
                .GroupBy(i => i.RescueRequestId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.CreatedAt).First());
            var orderedIds = preview.ProposedRequestIdsInOrder
                .Where(itemsByRequestId.ContainsKey)
                .Concat(activeBatch.Items.Where(i => !preview.ProposedRequestIdsInOrder.Contains(i.RescueRequestId))
                    .OrderBy(i => i.SequenceOrder)
                    .Select(i => i.RescueRequestId))
                .Distinct()
                .ToList();

            return await ReorderBatchQueueAsync(dto.TeamId, new ReorderRescueBatchRequestDto
            {
                RequestIdsInOrder = orderedIds
            }, cancellationToken);
        }

        public async Task<PaginatedDispatchCandidatesResponseDto> GetDispatchCandidatesAsync(
            GetDispatchCandidatesRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            await EnsureRescueTeamTypeAsync(dto.TeamId, cancellationToken);

            var pageNumber = dto.PageNumber <= 0 ? 1 : dto.PageNumber;
            var pageSize = dto.PageSize <= 0 ? 20 : dto.PageSize;

            var allRequests = await _unitOfWork.RescueRequests.GetAllAsync(cancellationToken);
            var activeBatches = await _unitOfWork.RescueBatches.GetAllActiveWithItemsAsync(cancellationToken);

            var teamHasActiveBatch = activeBatches.Any(b => b.TeamId == dto.TeamId);
            if (!teamHasActiveBatch)
            {
                return new PaginatedDispatchCandidatesResponseDto
                {
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Data = new List<DispatchCandidateResponseDto>()
                };
            }

            var filtered = allRequests
                .Where(r => string.IsNullOrWhiteSpace(dto.Search) ||
                            (r.Address ?? string.Empty).Contains(dto.Search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                            (r.Description ?? string.Empty).Contains(dto.Search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                            (r.ReporterFullName ?? string.Empty).Contains(dto.Search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                            (r.ReporterPhone ?? string.Empty).Contains(dto.Search.Trim(), StringComparison.OrdinalIgnoreCase))
                .Select(r =>
                {
                    var eligibility = EvaluateDispatchEligibility(r, dto.TeamId, activeBatches);

                    return new DispatchCandidateResponseDto
                    {
                        RequestId = r.RequestId,
                        UserName = r.ReporterUser?.UserName ?? r.ReporterFullName,
                        ReporterFullName = r.ReporterFullName,
                        ReporterPhone = r.ReporterPhone,
                        RescueRequestType = r.RescueRequestType.ToString(),
                        RescueRequestStatus = r.RescueRequestStatus.ToString(),
                        PriorityPoint = r.PriorityPoint,
                        PriorityLevel = r.RescuePriorityLevel.ToString(),
                        Address = r.Address,
                        Latitude = r.Latitude,
                        Longitude = r.Longitude,
                        AlreadyAssignedTeamId = eligibility.AssignedTeamId,
                        IsInOtherActiveBatch = eligibility.IsInOtherActiveBatch,
                        CanDispatch = eligibility.CanDispatch,
                        DispatchBlockReason = eligibility.BlockReason
                    };
                })
                .ToList();

            var totalCount = filtered.Count;
            var pageData = filtered
                .OrderByDescending(x => x.PriorityPoint ?? 0)
                .ThenBy(x => x.RescueRequestType)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedDispatchCandidatesResponseDto
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = pageData
            };
        }

        public async Task<BulkAssignRescueTeamResponseDto> AssignTeamToMultipleRescueRequestsAsync(
            AssignRescueTeamBulkRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            await EnsureRescueTeamTypeAsync(dto.TeamId, cancellationToken);

            var requestIds = (dto.RequestIds ?? new List<Guid>())
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var response = new BulkAssignRescueTeamResponseDto
            {
                TeamId = dto.TeamId,
                TotalRequested = requestIds.Count
            };

            foreach (var requestId in requestIds)
            {
                try
                {
                    await AssignTeamToRescueAsync(
                        requestId,
                        new AssignRescueTeamRequestDto
                        {
                            TeamId = dto.TeamId,
                            Note = dto.Note
                        },
                        cancellationToken);

                    response.SuccessRequestIds.Add(requestId);
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    response.Failures.Add(new BulkAssignFailureItemDto
                    {
                        RequestId = requestId,
                        Reason = ex.Message
                    });
                }
            }

            response.SuccessCount = response.SuccessRequestIds.Count;
            response.FailedCount = response.Failures.Count;

            return response;
        }

        public async Task<RescueRequestResponseDto> CompleteRescueOperationAsync(
            Guid requestId,
            Guid operationId,
            CompleteRescueOperationRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            var request = await _unitOfWork.RescueRequests.GetByIdForCompletionAsync(requestId, cancellationToken);
            if (request == null)
                throw new InvalidOperationException($"Rescue request {requestId} not found");

            var operation = request.RescueOperations
                .FirstOrDefault(o => o.RescueOperationId == operationId);

            if (operation == null)
                throw new InvalidOperationException("Rescue operation not found for this request.");

            if (!operation.TeamId.HasValue)
                throw new InvalidOperationException("Operation chưa được gán team, không thể hoàn tất.");

            var currentUserId = _currentUserService.UserId;
            if (!currentUserId.HasValue)
                throw new UnauthorizedAccessException("User not authenticated.");

            var teamLeaderId = await _unitOfWork.Teams.GetQueryable()
                .Where(t => t.TeamId == operation.TeamId.Value)
                .Select(t => t.LeaderId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!teamLeaderId.HasValue)
                throw new InvalidOperationException("Team được gán cho operation không tồn tại.");

            if (teamLeaderId.Value != currentUserId.Value)
                throw new UnauthorizedAccessException("Chỉ team leader của operation mới được xác nhận hoàn tất cứu hộ.");

            if (dto?.Attachments == null || dto.Attachments.Count == 0)
                throw new InvalidOperationException("Completing rescue requires at least one image evidence.");

            var now = DateTime.UtcNow;

            await _unitOfWork.RescueRequests.DetachTrackedAttachmentsAsync(request.RequestId, cancellationToken);

            var completionAttachments = dto.Attachments.Select(attachment => new Attachment
            {
                AttachmentId = Guid.NewGuid(),
                RequestId = request.RequestId,
                FileUrl = attachment.FileUrl,
                ContentType = attachment.ContentType,
                AttachmentType = AttachmentType.CompletionEvidence,
                UploadedAt = now
            }).ToList();

            foreach (var attachment in completionAttachments)
            {
                await _unitOfWork.Attachments.AddAsync(attachment);
            }

            operation.Status = RescueOperationStatus.RescueCompleted;
            operation.EndedAt = now;

            foreach (var vehicleId in GetAssignedVehicles(operation).Select(v => v.VehicleId).Distinct())
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
                if (vehicle != null && !vehicle.IsDeleted)
                {
                    vehicle.Status = VehicleStatus.Free;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Note))
            {
                operation.Note = string.IsNullOrWhiteSpace(operation.Note)
                    ? dto.Note
                    : $"{operation.Note}{Environment.NewLine}{dto.Note}";
            }

            request.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                await CompleteBatchItemAndAdvanceQueueAsync(
                    operation.TeamId.Value,
                    request.RequestId,
                    now,
                    cancellationToken);

                if (request.RescueOperations.All(o =>
                        o.Status == RescueOperationStatus.RescueCompleted ||
                        o.Status == RescueOperationStatus.Cancelled))
                {
                    request.RescueRequestStatus = RescueRequestStatus.Completed;
                }

                request.UpdatedAt = now;

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
            }

            return await GetRescueRequestByIdAsync(requestId, cancellationToken);
        }

        private async Task<Guid?> ResolveCampaignIdForRescueRequestAsync(
            DateTime requestCreatedAt,
            CancellationToken cancellationToken)
        {
            // Backend tự động gắn campaign Rescue đang diễn ra (nếu có)
            var campaigns = await _unitOfWork.Campaigns.GetAllAsync();
            var activeRescueCampaign = campaigns
                .Where(c => c.Type == CampaignType.Rescue)
                .Where(c => requestCreatedAt >= c.StartDate && requestCreatedAt <= c.EndDate)
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefault();

            return activeRescueCampaign?.CampaignId;
        }

        private async Task EnsureActiveBatchAndAppendRequestAsync(
            Guid teamId,
            Guid requestId,
            CancellationToken cancellationToken)
        {
            var activeBatch = await _unitOfWork.RescueBatches.GetActiveByTeamIdAsync(teamId, cancellationToken);

            if (activeBatch == null)
            {
                activeBatch = new RescueBatch
                {
                    RescueBatchId = Guid.NewGuid(),
                    TeamId = teamId,
                    IsActive = true,
                    Status = RescueBatchStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.RescueBatches.AddAsync(activeBatch);
            }

            var alreadyExists = activeBatch.Items.Any(i => i.RescueRequestId == requestId);
            if (alreadyExists)
            {
                return;
            }

            var maxOrder = await _unitOfWork.RescueBatchItems.GetMaxSequenceOrderAsync(
                activeBatch.RescueBatchId,
                cancellationToken);

            var nextOrder = maxOrder < 0 ? 0 : maxOrder + 1;

            var batchItem = new RescueBatchItem
            {
                RescueBatchItemId = Guid.NewGuid(),
                RescueBatchId = activeBatch.RescueBatchId,
                RescueRequestId = requestId,
                SequenceOrder = nextOrder,
                IsAutoAssigned = false,
                Status = nextOrder == 0 ? RescueBatchItemStatus.InProgress : RescueBatchItemStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RescueBatchItems.AddAsync(batchItem);
            activeBatch.Items.Add(batchItem);
        }

        private async Task<DispatchPreviewResponseDto> BuildSmartAssignPreviewAsync(
            Guid requestId,
            Guid teamId,
            bool allowPreempt,
            double normalNearRouteThresholdKm,
            double emergencyNearRouteThresholdKm,
            CancellationToken cancellationToken)
        {
            var request = await _unitOfWork.RescueRequests.GetByIdAsync(requestId, cancellationToken)
                ?? throw new InvalidOperationException($"Rescue request {requestId} not found");

            var activeBatch = await _unitOfWork.RescueBatches.GetActiveByTeamIdAsync(teamId, cancellationToken);
            var latestTracking = await _unitOfWork.TeamTrackingPoints.GetLatestPointAsync(teamId, cancellationToken);
            var allActiveBatches = await _unitOfWork.RescueBatches.GetAllActiveWithItemsAsync(cancellationToken);

            var preview = new DispatchPreviewResponseDto
            {
                RequestId = requestId,
                TeamId = teamId,
                RescueRequestType = request.RescueRequestType.ToString(),
                PriorityPoint = request.PriorityPoint,
                PriorityLevel = request.RescuePriorityLevel.ToString(),
                Eligible = true,
                RecommendedAction = "AssignQueueTail"
            };

            var eligibility = EvaluateDispatchEligibility(request, teamId, allActiveBatches);
            if (!eligibility.CanDispatch)
            {
                preview.Eligible = false;
                preview.Reasons.Add(eligibility.BlockReason ?? "Request cannot be dispatched.");
                return preview;
            }

            if (latestTracking == null)
            {
                preview.Eligible = false;
                preview.Reasons.Add("Team has no tracking location for smart assignment.");
                return preview;
            }

            preview.DistanceFromTeamKm = CalculateDistance(
                latestTracking.Latitude,
                latestTracking.Longitude,
                request.Latitude,
                request.Longitude);

            if (activeBatch == null || activeBatch.Items.Count == 0)
            {
                preview.RecommendedAction = request.RescueRequestType == RescueRequestType.Emergency
                    ? "AssignAsInProgress"
                    : "AssignAndInsertQueue";
                preview.RecommendedQueueIndex = 0;
                preview.ProposedRequestIdsInOrder.Add(requestId);
                return preview;
            }

            var currentInProgress = activeBatch.Items
                .FirstOrDefault(i => i.Status == RescueBatchItemStatus.InProgress)
                ?? activeBatch.Items.OrderBy(i => i.SequenceOrder).FirstOrDefault();

            preview.CurrentInProgressRequestId = currentInProgress?.RescueRequestId;
            preview.CurrentInProgressBatchItemId = currentInProgress?.RescueBatchItemId;

            var orderedActiveIds = activeBatch.Items
                .OrderBy(i => i.SequenceOrder)
                .Select(i => i.RescueRequestId)
                .ToList();

            if (currentInProgress?.RescueRequest != null)
            {
                preview.DistanceToCurrentInProgressKm = CalculateDistance(
                    request.Latitude,
                    request.Longitude,
                    currentInProgress.RescueRequest.Latitude,
                    currentInProgress.RescueRequest.Longitude);
            }

            var threshold = request.RescueRequestType == RescueRequestType.Emergency
                ? emergencyNearRouteThresholdKm
                : normalNearRouteThresholdKm;

            var thresholdMeters = threshold * 1000d;
            const int backtrackDetourThresholdMeters = 300;

            var usedGoongNearRouteMetric = false;
            var usedGoongDetourMetric = false;

            if (currentInProgress?.RescueRequest != null)
            {
                try
                {
                    var routeA = await _goongRouteService.GetRouteAsync(
                        latestTracking.Latitude,
                        latestTracking.Longitude,
                        currentInProgress.RescueRequest.Latitude,
                        currentInProgress.RescueRequest.Longitude,
                        cancellationToken: cancellationToken);

                    if (routeA != null && !string.IsNullOrWhiteSpace(routeA.OverviewPolyline))
                    {
                        preview.CurrentRoutePolyline = routeA.OverviewPolyline;
                        preview.CurrentRouteDistanceMeters = routeA.DistanceMeters;
                        preview.CurrentRouteDurationSeconds = routeA.DurationSeconds;

                        var routePoints = DecodePolyline(routeA.OverviewPolyline);
                        if (routePoints.Count > 0)
                        {
                            var minDistanceMeters = GetMinDistanceToRouteMeters(
                                request.Latitude,
                                request.Longitude,
                                routePoints);

                            preview.MinDistanceToCurrentRouteMeters = minDistanceMeters;
                            preview.IsNearCurrentRoute = minDistanceMeters <= thresholdMeters;
                            usedGoongNearRouteMetric = true;
                        }

                        var routeB = await _goongRouteService.GetRouteAsync(
                            latestTracking.Latitude,
                            latestTracking.Longitude,
                            request.Latitude,
                            request.Longitude,
                            cancellationToken: cancellationToken);

                        var routeC = await _goongRouteService.GetRouteAsync(
                            request.Latitude,
                            request.Longitude,
                            currentInProgress.RescueRequest.Latitude,
                            currentInProgress.RescueRequest.Longitude,
                            cancellationToken: cancellationToken);

                        if (routeA.DistanceMeters.HasValue && routeB?.DistanceMeters.HasValue == true && routeC?.DistanceMeters.HasValue == true)
                        {
                            preview.DetourMeters = Math.Max(0, routeB.DistanceMeters.Value + routeC.DistanceMeters.Value - routeA.DistanceMeters.Value);
                            preview.RequiresBacktrack = preview.DetourMeters.Value > backtrackDetourThresholdMeters;
                            usedGoongDetourMetric = true;
                        }

                        if (routeA.DurationSeconds.HasValue && routeB?.DurationSeconds.HasValue == true && routeC?.DurationSeconds.HasValue == true)
                        {
                            preview.DetourSeconds = Math.Max(0, routeB.DurationSeconds.Value + routeC.DurationSeconds.Value - routeA.DurationSeconds.Value);
                        }
                    }
                }
                catch (Exception) when (!cancellationToken.IsCancellationRequested)
                {
                }
            }

            if (!usedGoongNearRouteMetric)
            {
                preview.IsNearCurrentRoute = preview.DistanceToCurrentInProgressKm.HasValue &&
                                             preview.DistanceToCurrentInProgressKm.Value <= threshold;
            }

            if (!usedGoongDetourMetric)
            {
                preview.RequiresBacktrack = currentInProgress?.SequenceOrder == 0 &&
                                            preview.DistanceFromTeamKm.HasValue &&
                                            preview.DistanceToCurrentInProgressKm.HasValue &&
                                            preview.DistanceFromTeamKm.Value + 0.3 < preview.DistanceToCurrentInProgressKm.Value;
            }

            if (request.RescueRequestType == RescueRequestType.Emergency &&
                allowPreempt &&
                preview.IsNearCurrentRoute &&
                !preview.RequiresBacktrack)
            {
                preview.WillPreemptCurrentInProgress = true;
                preview.RecommendedAction = "AssignAsInProgress";
                preview.RecommendedQueueIndex = 0;
                preview.ProposedRequestIdsInOrder = new List<Guid> { requestId };
                preview.ProposedRequestIdsInOrder.AddRange(orderedActiveIds);
                preview.Reasons.Add("Emergency request is near current route and can preempt active mission.");
                return preview;
            }

            if (request.RescueRequestType == RescueRequestType.Normal)
            {
                var pendingIds = activeBatch.Items
                    .Where(i => i.Status != RescueBatchItemStatus.InProgress)
                    .OrderBy(i => i.SequenceOrder)
                    .Select(i => i.RescueRequestId)
                    .ToList();

                preview.RecommendedAction = preview.IsNearCurrentRoute
                    ? "AssignAndInsertQueue"
                    : "AssignQueueTail";
                preview.RecommendedQueueIndex = currentInProgress != null ? 1 : 0;
                preview.ProposedRequestIdsInOrder = currentInProgress != null
                    ? new List<Guid> { currentInProgress.RescueRequestId }
                    : new List<Guid>();

                var inserted = false;
                foreach (var pendingId in pendingIds)
                {
                    var pendingItem = activeBatch.Items.First(i => i.RescueRequestId == pendingId);
                    var pendingPriority = pendingItem.RescueRequest?.PriorityPoint ?? 0;
                    var newPriority = request.PriorityPoint ?? 0;
                    if (!inserted && (newPriority > pendingPriority))
                    {
                        preview.ProposedRequestIdsInOrder.Add(requestId);
                        inserted = true;
                    }

                    preview.ProposedRequestIdsInOrder.Add(pendingId);
                }

                if (!inserted)
                {
                    preview.ProposedRequestIdsInOrder.Add(requestId);
                }

                return preview;
            }

            preview.RecommendedAction = "AssignAndInsertQueue";
            preview.RecommendedQueueIndex = currentInProgress != null ? 1 : 0;
            preview.ProposedRequestIdsInOrder = currentInProgress != null
                ? new List<Guid> { currentInProgress.RescueRequestId, requestId }
                : new List<Guid> { requestId };
            preview.ProposedRequestIdsInOrder.AddRange(orderedActiveIds.Where(id => id != preview.CurrentInProgressRequestId));
            preview.Reasons.Add("Emergency request inserted right after current in-progress mission.");
            return preview;
        }

        private async Task CompleteBatchItemAndAdvanceQueueAsync(
            Guid teamId,
            Guid completedRequestId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var activeBatch = await _unitOfWork.RescueBatches.GetActiveByTeamIdAsync(teamId, cancellationToken);
            if (activeBatch == null)
            {
                return;
            }

            var completedItem = activeBatch.Items.FirstOrDefault(i => i.RescueRequestId == completedRequestId);
            if (completedItem != null)
            {
                completedItem.Status = RescueBatchItemStatus.Done;
            }

            var nextPending = activeBatch.Items
                .Where(i => i.Status == RescueBatchItemStatus.Pending)
                .OrderBy(i => i.SequenceOrder)
                .FirstOrDefault();

            if (nextPending != null)
            {
                nextPending.Status = RescueBatchItemStatus.InProgress;

                var nextRequest = nextPending.RescueRequest;
                if (nextRequest != null)
                {
                    var nextStationOperation = nextRequest.RescueOperations
                        .Where(o => o.TeamId == teamId && o.ReliefStationId.HasValue)
                        .OrderByDescending(o => o.StartedAt)
                        .FirstOrDefault();

                    if (nextStationOperation != null)
                    {
                        nextRequest.RescueRequestStatus = RescueRequestStatus.InProgress;
                        nextRequest.UpdatedAt = now;
                    }
                }
            }

            var allDoneOrCancelled = activeBatch.Items.All(i =>
                i.Status == RescueBatchItemStatus.Done || i.Status == RescueBatchItemStatus.Cancelled);

            if (allDoneOrCancelled)
            {
                activeBatch.IsActive = false;
                activeBatch.Status = RescueBatchStatus.Completed;
                activeBatch.ClosedAt = now;
            }
        }

        private DispatchEligibilityResult EvaluateDispatchEligibility(
            Domain.Entities.RescueRequest request,
            Guid teamId,
            IEnumerable<RescueBatch> activeBatches)
        {
            if (request.RescueRequestStatus == RescueRequestStatus.Completed)
            {
                return new DispatchEligibilityResult
                {
                    CanDispatch = false,
                    BlockReason = "Request đã hoàn thành."
                };
            }

            if (request.RescueRequestStatus == RescueRequestStatus.Cancelled)
            {
                return new DispatchEligibilityResult
                {
                    CanDispatch = false,
                    BlockReason = "Request đã bị hủy."
                };
            }

            var assignedTeamId = request.RescueOperations
                .Where(o => o.TeamId.HasValue)
                .OrderByDescending(o => o.StartedAt)
                .Select(o => o.TeamId)
                .FirstOrDefault();

            if (assignedTeamId.HasValue && assignedTeamId.Value != teamId)
            {
                return new DispatchEligibilityResult
                {
                    CanDispatch = false,
                    AssignedTeamId = assignedTeamId,
                    BlockReason = "Request đã được assign cho team khác."
                };
            }

            var activeBatchTeamIds = activeBatches
                .Where(b => b.Items.Any(i => i.RescueRequestId == request.RequestId))
                .Select(b => b.TeamId)
                .Distinct()
                .ToList();

            var isInOtherActiveBatch = activeBatchTeamIds.Any(t => t != teamId);
            if (isInOtherActiveBatch)
            {
                return new DispatchEligibilityResult
                {
                    CanDispatch = false,
                    AssignedTeamId = assignedTeamId,
                    IsInOtherActiveBatch = true,
                    BlockReason = "Request đang nằm trong active batch của team khác."
                };
            }

            return new DispatchEligibilityResult
            {
                CanDispatch = true,
                AssignedTeamId = assignedTeamId,
                IsInOtherActiveBatch = false
            };
        }

        private sealed class DispatchEligibilityResult
        {
            public bool CanDispatch { get; set; }
            public Guid? AssignedTeamId { get; set; }
            public bool IsInOtherActiveBatch { get; set; }
            public string? BlockReason { get; set; }
        }

        // ... rest of the existing methods unchanged (GetRescueRequestByIdAsync, GetRescueRequestsAsync,
        // VerifyRescueRequestAsync, CalculatePriorityAsync, DispatchToStationsAsync, helpers etc.)
        // Note: CalculatePriorityAsync remains as fallback automatic scoring when user doesn't select criteria.

        /// <summary>Lấy chi tiết yêu cầu cứu hộ</summary>
        public async Task<RescueRequestResponseDto> GetRescueRequestByIdAsync(
            Guid requestId,
            CancellationToken cancellationToken = default)
        {
            var request = await _unitOfWork.RescueRequests.GetByIdAsync(requestId, cancellationToken);

            if (request == null)
                throw new InvalidOperationException($"Rescue request {requestId} not found");

            return MapToResponseDto(request);
        }

        public async Task<PaginatedRescueRequestResponseDto> GetRescueRequestsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            int? statusFilter = null,
            CancellationToken cancellationToken = default)
        {
            var requests = await _unitOfWork.RescueRequests.GetAllAsync(cancellationToken);

            if (statusFilter.HasValue)
            {
                requests = requests.Where(r => (int)r.RescueRequestStatus == statusFilter.Value).ToList();
            }

            var totalCount = requests.Count;
            var paginatedRequests = requests
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PaginatedRescueRequestResponseDto
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = paginatedRequests.Select(r => MapToResponseDto(r)).ToList()
            };

            return response;
        }

        public async Task<PaginatedRescueRequestResponseDto> SearchRescueRequestsAsync(
            SearchRescueRequestDto request,
            CancellationToken cancellationToken = default)
        {
            request ??= new SearchRescueRequestDto();

            var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var allData = await GetRescueRequestsAsync(
                pageNumber: 1,
                pageSize: int.MaxValue,
                statusFilter: request.StatusFilter,
                cancellationToken: cancellationToken);

            var query = (request.Search ?? string.Empty).Trim();
            IEnumerable<RescueRequestResponseDto> filtered = allData.Data;

            if (request.VerificationStatus.HasValue)
            {
                filtered = filtered.Where(r => r.Verifications.Any(v => (int)v.Status == request.VerificationStatus.Value));
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                filtered = filtered.Where(r =>
                    (!string.IsNullOrWhiteSpace(r.ReporterFullName) && r.ReporterFullName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(r.ReporterPhone) && r.ReporterPhone.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(r.Address) && r.Address.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(r.Description) && r.Description.Contains(query, StringComparison.OrdinalIgnoreCase)));
            }

            var filteredList = filtered.ToList();
            var totalCount = filteredList.Count;
            var paged = filteredList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedRescueRequestResponseDto
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = paged
            };
        }

        public async Task<PaginatedRescueRequestResponseDto> GetCurrentModeratorStationRequestsAsync(
            string? search,
            int? statusFilter,
            int? verificationStatus,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUserService.UserId
                ?? throw new UnauthorizedAccessException("Current user is not authenticated.");

            var moderatorProfile = await _unitOfWork.ModeratorProfiles.GetByUserIdAsync(currentUserId, cancellationToken)
                ?? throw new InvalidOperationException("Moderator profile not found.");

            if (!moderatorProfile.ReliefStationId.HasValue)
                throw new InvalidOperationException("Current moderator is not assigned to any relief station.");

            var stationId = moderatorProfile.ReliefStationId.Value;

            var allData = await GetRescueRequestsAsync(
                pageNumber: 1,
                pageSize: int.MaxValue,
                statusFilter: statusFilter,
                cancellationToken: cancellationToken);

            var stationOperations = await _unitOfWork.RescueOperations.GetByStationIdAsync(stationId, cancellationToken);
            var requestIdsOfStation = stationOperations
                .Select(o => o.RescueRequestId)
                .Distinct()
                .ToHashSet();

            IEnumerable<RescueRequestResponseDto> filtered = allData.Data.Where(r => requestIdsOfStation.Contains(r.RequestId));

            if (verificationStatus.HasValue)
            {
                filtered = filtered.Where(r => r.Verifications.Any(v => (int)v.Status == verificationStatus.Value));
            }

            var query = (search ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(query))
            {
                filtered = filtered.Where(r =>
                    (!string.IsNullOrWhiteSpace(r.ReporterFullName) && r.ReporterFullName.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(r.ReporterPhone) && r.ReporterPhone.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(r.Address) && r.Address.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(r.Description) && r.Description.Contains(query, StringComparison.OrdinalIgnoreCase)));
            }

            var safePageNumber = pageNumber <= 0 ? 1 : pageNumber;
            var safePageSize = pageSize <= 0 ? 10 : pageSize;

            var filteredList = filtered.ToList();
            var totalCount = filteredList.Count;
            var paged = filteredList
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .ToList();

            return new PaginatedRescueRequestResponseDto
            {
                TotalCount = totalCount,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                Data = paged
            };
        }

        public async Task<RescueBatchQueueResponseDto?> GetActiveBatchByTeamAsync(
            Guid teamId,
            CancellationToken cancellationToken = default)
        {
            var activeBatch = await _unitOfWork.RescueBatches.GetActiveByTeamIdAsync(teamId, cancellationToken);
            if (activeBatch == null)
            {
                return null;
            }

            return MapToBatchQueueResponseDto(activeBatch);
        }

        public async Task<RescueBatchQueueResponseDto> ReorderBatchQueueAsync(
            Guid teamId,
            ReorderRescueBatchRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            var activeBatch = await _unitOfWork.RescueBatches.GetActiveByTeamIdAsync(teamId, cancellationToken);
            if (activeBatch == null)
            {
                throw new InvalidOperationException("No active rescue batch found for this team.");
            }

            if (dto?.RequestIdsInOrder == null || dto.RequestIdsInOrder.Count == 0)
            {
                throw new InvalidOperationException("RequestIdsInOrder is required.");
            }

            var providedIds = dto.RequestIdsInOrder;
            var duplicateProvidedIds = providedIds
                .GroupBy(id => id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateProvidedIds.Count > 0)
            {
                throw new InvalidOperationException("RequestIdsInOrder contains duplicate request ids.");
            }

            var batchItemsByRequestId = activeBatch.Items.ToDictionary(i => i.RescueRequestId, i => i);
            var invalidIds = providedIds.Where(id => !batchItemsByRequestId.ContainsKey(id)).ToList();
            if (invalidIds.Count > 0)
            {
                throw new InvalidOperationException("One or more request ids are not in the active batch.");
            }

            var remainingItems = activeBatch.Items
                .Where(i => !providedIds.Contains(i.RescueRequestId))
                .OrderBy(i => i.SequenceOrder)
                .ToList();

            var orderedItems = providedIds
                .Select(id => batchItemsByRequestId[id])
                .Concat(remainingItems)
                .ToList();

            for (var i = 0; i < orderedItems.Count; i++)
            {
                var item = orderedItems[i];
                item.SequenceOrder = i;

                if (item.Status == RescueBatchItemStatus.Done || item.Status == RescueBatchItemStatus.Cancelled)
                {
                    continue;
                }

                item.Status = i == 0
                    ? RescueBatchItemStatus.InProgress
                    : RescueBatchItemStatus.Pending;
            }

            await RecalculateBatchEtaFromLatestTrackingAsync(teamId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapToBatchQueueResponseDto(activeBatch);
        }

        public async Task RecalculateActiveBatchEtaAsync(
            Guid teamId,
            CancellationToken cancellationToken = default)
        {
            await RecalculateBatchEtaFromLatestTrackingAsync(teamId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<RescueRequestResponseDto> UpdateRescueOperationStatusAsync(
            Guid requestId,
            Guid operationId,
            UpdateRescueOperationStatusRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            var request = await _unitOfWork.RescueRequests.GetByIdAsync(requestId, cancellationToken);
            if (request == null)
            {
                throw new InvalidOperationException($"Rescue request {requestId} not found");
            }

            var operation = request.RescueOperations.FirstOrDefault(o => o.RescueOperationId == operationId);
            if (operation == null)
            {
                throw new InvalidOperationException("Rescue operation not found for this request.");
            }

            var allowedStatuses = new HashSet<RescueOperationStatus>
            {
                RescueOperationStatus.EnRoute,
                RescueOperationStatus.Rescuing,
                RescueOperationStatus.Returning,
                RescueOperationStatus.Closed,
                RescueOperationStatus.Cancelled
            };

            if (!allowedStatuses.Contains(dto.Status))
            {
                throw new InvalidOperationException("Status is not allowed for this API.");
            }

            var now = DateTime.UtcNow;
            operation.Status = dto.Status;

            if (dto.Status == RescueOperationStatus.Closed || dto.Status == RescueOperationStatus.Cancelled)
            {
                operation.EndedAt = now;

                foreach (var vehicleId in GetAssignedVehicles(operation).Select(v => v.VehicleId).Distinct())
                {
                    var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
                    if (vehicle != null && !vehicle.IsDeleted)
                    {
                        vehicle.Status = VehicleStatus.Free;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Note))
            {
                operation.Note = string.IsNullOrWhiteSpace(operation.Note)
                    ? dto.Note
                    : $"{operation.Note}{Environment.NewLine}{dto.Note}";
            }

            if (dto.Status == RescueOperationStatus.Rescuing ||
                dto.Status == RescueOperationStatus.EnRoute ||
                dto.Status == RescueOperationStatus.Returning)
            {
                request.RescueRequestStatus = RescueRequestStatus.InProgress;
            }
            else if (dto.Status == RescueOperationStatus.Closed)
            {
                var allClosedOrCancelled = request.RescueOperations.All(o =>
                    o.Status == RescueOperationStatus.Closed ||
                    o.Status == RescueOperationStatus.Cancelled);

                if (allClosedOrCancelled)
                {
                    request.RescueRequestStatus = RescueRequestStatus.Completed;
                }
            }
            else if (dto.Status == RescueOperationStatus.Cancelled)
            {
                var allClosedOrCancelled = request.RescueOperations.All(o =>
                    o.Status == RescueOperationStatus.Closed ||
                    o.Status == RescueOperationStatus.Cancelled);

                if (allClosedOrCancelled)
                {
                    var hasAnyClosedOrCompletedOperation = request.RescueOperations.Any(o =>
                        o.Status == RescueOperationStatus.Closed ||
                        o.Status == RescueOperationStatus.RescueCompleted);

                    request.RescueRequestStatus = hasAnyClosedOrCompletedOperation
                        ? RescueRequestStatus.Completed
                        : RescueRequestStatus.Cancelled;
                }
            }

            request.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (dto.Status == RescueOperationStatus.EnRoute)
            {
                await NotifyReporterOnRescueTeamEnRouteAsync(request, cancellationToken);
            }

            return await GetRescueRequestByIdAsync(requestId, cancellationToken);
        }

        private async Task NotifyModeratorsOnRescueRequestCreatedAsync(
            Guid reliefStationId,
            Domain.Entities.RescueRequest rescueRequest,
            CancellationToken cancellationToken)
        {
            var moderators = await _unitOfWork.ModeratorProfiles.GetActiveByStationIdAsync(reliefStationId, cancellationToken);
            var moderatorIds = moderators.Select(m => m.UserId).Distinct().ToList();

            if (moderatorIds.Count == 0)
            {
                return;
            }

            var title = "Có yêu cầu cứu hộ mới";
            var message = $"Rescue request mới tại trạm của bạn: {rescueRequest.Address ?? "(không có địa chỉ)"}.";
            var metadataJson = BuildRescueRequestNotificationMetadataJson(rescueRequest);

            await _notificationService.CreateManyAndPushAsync(
                moderatorIds,
                NotificationType.RescueRequestCreated,
                title,
                message,
                rescueRequest.RequestId,
                nameof(Domain.Entities.RescueRequest),
                metadataJson,
                cancellationToken);
        }

        private async Task NotifyReporterOnRescueRequestVerifiedAsync(
            Domain.Entities.RescueRequest request,
            RequestVerificationStatus verificationStatus,
            CancellationToken cancellationToken)
        {
            if (!request.ReporterUserId.HasValue)
            {
                return;
            }

            var title = verificationStatus == RequestVerificationStatus.Approved
                ? "Yêu cầu cứu hộ đã được duyệt"
                : verificationStatus == RequestVerificationStatus.Rejected
                    ? "Yêu cầu cứu hộ bị từ chối"
                    : "Yêu cầu cứu hộ đã được cập nhật xác minh";

            var message = verificationStatus == RequestVerificationStatus.Approved
                ? "Yêu cầu cứu hộ của bạn đã được xác minh và đang chờ điều phối."
                : verificationStatus == RequestVerificationStatus.Rejected
                    ? "Yêu cầu cứu hộ của bạn đã bị từ chối. Vui lòng kiểm tra lại thông tin."
                    : "Trạng thái xác minh yêu cầu cứu hộ của bạn đã được cập nhật.";

            await _notificationService.CreateAndPushAsync(
                request.ReporterUserId.Value,
                NotificationType.RescueRequestVerified,
                title,
                message,
                request.RequestId,
                nameof(Domain.Entities.RescueRequest),
                null,
                cancellationToken);
        }

        private async Task NotifyOnRescueRequestAssignedAsync(
            Domain.Entities.RescueRequest request,
            Guid teamId,
            CancellationToken cancellationToken)
        {
            var title = "Yêu cầu cứu hộ đã được điều phối team";
            var message = "Đội cứu hộ đã được điều phối cho yêu cầu của bạn.";

            if (request.ReporterUserId.HasValue)
            {
                await _notificationService.CreateAndPushAsync(
                    request.ReporterUserId.Value,
                    NotificationType.RescueRequestAssigned,
                    title,
                    message,
                    request.RequestId,
                    nameof(Domain.Entities.RescueRequest),
                    null,
                    cancellationToken);
            }

            var teamMemberUserIds = _unitOfWork.TeamMembers.GetQueryable()
                .Where(tm => tm.TeamId == teamId)
                .Select(tm => tm.UserId)
                .Distinct()
                .ToList();

            if (teamMemberUserIds.Count > 0)
            {
                await _notificationService.CreateManyAndPushAsync(
                    teamMemberUserIds,
                    NotificationType.RescueRequestAssigned,
                    "Bạn được điều phối cứu hộ",
                    $"Team của bạn vừa được điều phối tới yêu cầu cứu hộ tại {request.Address ?? "(không có địa chỉ)"}.",
                    request.RequestId,
                    nameof(Domain.Entities.RescueRequest),
                    null,
                    cancellationToken);
            }
        }

        private async Task NotifyReporterOnRescueTeamEnRouteAsync(
            Domain.Entities.RescueRequest request,
            CancellationToken cancellationToken)
        {
            if (!request.ReporterUserId.HasValue)
            {
                return;
            }

            await _notificationService.CreateAndPushAsync(
                request.ReporterUserId.Value,
                NotificationType.RescueRequestInProgress,
                "Đội cứu hộ đang di chuyển",
                "Đội cứu hộ đang trên đường đến vị trí của bạn.",
                request.RequestId,
                nameof(Domain.Entities.RescueRequest),
                null,
                cancellationToken);
        }

        private static string? BuildRescueRequestNotificationMetadataJson(Domain.Entities.RescueRequest rescueRequest)
        {
            var thumbnailUrls = rescueRequest.Attachments?
                .OrderBy(a => a.UploadedAt)
                .Select(a => a.FileUrl)
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Take(3)
                .ToList() ?? new List<string>();

            var attachmentCount = rescueRequest.Attachments?.Count ?? 0;
            if (attachmentCount == 0 && thumbnailUrls.Count == 0)
            {
                return null;
            }

            var metadata = new NotificationMetadataDto
            {
                AttachmentCount = attachmentCount,
                ThumbnailUrls = thumbnailUrls
            };

            return JsonSerializer.Serialize(metadata);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static List<(double lat, double lng)> DecodePolyline(string encoded)
        {
            var points = new List<(double lat, double lng)>();
            if (string.IsNullOrWhiteSpace(encoded))
            {
                return points;
            }

            var index = 0;
            var lat = 0;
            var lng = 0;

            while (index < encoded.Length)
            {
                var result = 0;
                var shift = 0;
                int b;
                do
                {
                    if (index >= encoded.Length)
                        return points;
                    b = encoded[index++] - 63;
                    result |= (b & 0x1F) << shift;
                    shift += 5;
                }
                while (b >= 0x20);

                var dLat = (result & 1) != 0 ? ~(result >> 1) : (result >> 1);
                lat += dLat;

                result = 0;
                shift = 0;
                do
                {
                    if (index >= encoded.Length)
                        return points;
                    b = encoded[index++] - 63;
                    result |= (b & 0x1F) << shift;
                    shift += 5;
                }
                while (b >= 0x20);

                var dLng = (result & 1) != 0 ? ~(result >> 1) : (result >> 1);
                lng += dLng;

                points.Add((lat / 1E5, lng / 1E5));
            }

            return points;
        }

        private static double GetMinDistanceToRouteMeters(
            double pointLat,
            double pointLng,
            IReadOnlyList<(double lat, double lng)> routePoints)
        {
            if (routePoints == null || routePoints.Count == 0)
                return double.MaxValue;

            if (routePoints.Count == 1)
                return GetHaversineDistanceMeters(pointLat, pointLng, routePoints[0].lat, routePoints[0].lng);

            var min = double.MaxValue;
            for (var i = 0; i < routePoints.Count - 1; i++)
            {
                var a = routePoints[i];
                var b = routePoints[i + 1];
                var d = GetPointToSegmentDistanceMeters(pointLat, pointLng, a.lat, a.lng, b.lat, b.lng);
                if (d < min)
                {
                    min = d;
                }
            }

            return min;
        }

        private static double GetPointToSegmentDistanceMeters(
            double pLat,
            double pLng,
            double aLat,
            double aLng,
            double bLat,
            double bLng)
        {
            const double EarthRadiusMeters = 6371000d;
            var refLatRad = (pLat + aLat + bLat) / 3d * Math.PI / 180d;

            (double x, double y) ToXY(double lat, double lng)
            {
                var x = lng * Math.PI / 180d * EarthRadiusMeters * Math.Cos(refLatRad);
                var y = lat * Math.PI / 180d * EarthRadiusMeters;
                return (x, y);
            }

            var p = ToXY(pLat, pLng);
            var a = ToXY(aLat, aLng);
            var b = ToXY(bLat, bLng);

            var abx = b.x - a.x;
            var aby = b.y - a.y;
            var apx = p.x - a.x;
            var apy = p.y - a.y;
            var ab2 = abx * abx + aby * aby;

            if (ab2 <= double.Epsilon)
                return Math.Sqrt(apx * apx + apy * apy);

            var t = (apx * abx + apy * aby) / ab2;
            t = Math.Max(0d, Math.Min(1d, t));

            var cx = a.x + t * abx;
            var cy = a.y + t * aby;

            var dx = p.x - cx;
            var dy = p.y - cy;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static double GetHaversineDistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000d;
            var dLat = (lat2 - lat1) * Math.PI / 180d;
            var dLon = (lon2 - lon1) * Math.PI / 180d;
            var a = Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d) +
                    Math.Cos(lat1 * Math.PI / 180d) * Math.Cos(lat2 * Math.PI / 180d) *
                    Math.Sin(dLon / 2d) * Math.Sin(dLon / 2d);
            var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
            return R * c;
        }

        private RescuePriorityLevel CalculatePriorityLevel(int totalPoints)
        {
            if (totalPoints >= 81)
                return RescuePriorityLevel.Critical; // Critical
            else if (totalPoints >= 51)
                return RescuePriorityLevel.High; // High
            else if (totalPoints >= 21)
                return RescuePriorityLevel.Medium; // Medium
            else
                return RescuePriorityLevel.Low; // Low
        }

        private async Task<ReliefStation> SelectStationForRescueRequestAsync(
            double requestLatitude,
            double requestLongitude,
            CancellationToken cancellationToken)
        {
            var stations = await _unitOfWork.ReliefStations.GetAllAsync();
            var activeStations = stations
                .Where(s => s.ReliefStationStatus == ReliefStationStatus.Active)
                .ToList();

            if (activeStations.Count == 0)
                throw new InvalidOperationException("No active relief station available for dispatch.");

            var scoredStations = await GetStationDistancesWithFallbackAsync(
                requestLatitude,
                requestLongitude,
                activeStations,
                cancellationToken);

            var withinCoverage = scoredStations
                .Where(s => s.DistanceKm <= s.Station.CoverageRadiusKm)
                .OrderBy(s => s.DistanceKm)
                .FirstOrDefault();

            return withinCoverage?.Station
                ?? scoredStations.OrderBy(s => s.DistanceKm).First().Station;
        }

        private async Task<List<StationDistanceScore>> GetStationDistancesWithFallbackAsync(
            double requestLatitude,
            double requestLongitude,
            List<ReliefStation> stations,
            CancellationToken cancellationToken)
        {
            var fallback = stations
                .Select(s => new StationDistanceScore
                {
                    Station = s,
                    DistanceKm = CalculateDistance(requestLatitude, requestLongitude, s.Latitude, s.Longitude)
                })
                .ToList();

            try
            {
                var destinations = stations
                    .Select(s => (s.Latitude, s.Longitude))
                    .ToList();

                var matrix = await _goongDistanceService.GetDistanceMatrixAsync(
                    requestLatitude,
                    requestLongitude,
                    destinations,
                    cancellationToken: cancellationToken);

                if (matrix?.Elements == null || matrix.Elements.Count == 0)
                    return fallback;

                var scored = new List<StationDistanceScore>(stations.Count);

                for (var i = 0; i < stations.Count; i++)
                {
                    var station = stations[i];
                    var element = i < matrix.Elements.Count ? matrix.Elements[i] : null;

                    var hasValidGoongDistance = element != null
                        && string.Equals(element.Status, "OK", StringComparison.OrdinalIgnoreCase)
                        && element.DistanceMeters.HasValue;

                    var distanceKm = hasValidGoongDistance
                        ? element!.DistanceMeters!.Value / 1000d
                        : CalculateDistance(requestLatitude, requestLongitude, station.Latitude, station.Longitude);

                    scored.Add(new StationDistanceScore
                    {
                        Station = station,
                        DistanceKm = distanceKm
                    });
                }

                return scored;
            }
            catch
            {
                return fallback;
            }
        }

        private RescueRequestResponseDto MapToResponseDto(
            Domain.Entities.RescueRequest request,
            RescueRouteSnapshot? routeSnapshot = null)
        {
            return new RescueRequestResponseDto
            {
                RequestId = request.RequestId,
                DisasterType = request.DisasterType.ToString(),
                RescueRequestType = request.RescueRequestType.ToString(),
                Description = request.Description,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Address = request.Address,
                ReporterFullName = request.ReporterFullName,
                ReporterPhone = request.ReporterPhone,
                Priority = request.PriorityPoint,
                PriorityLevel = request.RescuePriorityLevel,
                RescueRequestStatus = request.RescueRequestStatus.ToString(),
                DispatchMode = request.DispatchMode.ToString(),
                Note = request.Note,
                WeatherCondition = request.WeatherCondition,
                WeatherTempC = request.WeatherTempC,
                WeatherWindKph = request.WeatherWindKph,
                WeatherPrecipMm = request.WeatherPrecipMm,
                WeatherVisibilityKm = request.WeatherVisibilityKm,
                WeatherRiskScore = request.WeatherRiskScore,
                WeatherRiskLevel = request.WeatherRiskLevel,
                WeatherObservedAt = request.WeatherObservedAt,
                CampaignId = request.CampaignId,
                CampaignName = request.Campaign?.Name,
                StationToRequestDistanceKm = routeSnapshot?.DistanceKm ?? request.StationToRequestDistanceKm,
                StationToRequestDurationMinutes = routeSnapshot?.DurationMinutes ?? request.StationToRequestDurationMinutes,
                StationToRequestDistanceMeters = routeSnapshot?.DistanceMeters ?? request.StationToRequestDistanceMeters,
                StationToRequestDurationSeconds = routeSnapshot?.DurationSeconds ?? request.StationToRequestDurationSeconds,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                Attachments = request.Attachments.Select(a => new AttachmentResponseDto
                {
                    AttachmentId = a.AttachmentId,
                    FileUrl = a.FileUrl,
                    ContentType = a.ContentType,
                    AttachmentType = a.AttachmentType.ToString(),
                    UploadedAt = a.UploadedAt
                }).ToList(),
                PriorityDetails = request.RescueRequestPriorities.Select(rp => new RescueRequestPriorityDto
                {
                    CriteriaName = rp.PriorityCriteria?.Name ?? string.Empty,
                    AppliedPoint = rp.AppliedPoint,
                    Description = rp.PriorityCriteria?.Description ?? string.Empty
                }).ToList(),
                RescueOperations = request.RescueOperations.Select(ro => new RescueOperationDto
                {
                    RescueOperationId = ro.RescueOperationId,
                    TeamId = ro.TeamId,
                    VehicleId = ro.VehicleId,
                    TeamName = ro.Team?.Name,
                    VehicleName = ro.Vehicle?.VehicleType?.TypeName,
                    VehicleLicensePlate = ro.Vehicle?.LicensePlate,
                    Vehicles = MapAssignedVehicles(ro),
                    Supplies = MapOperationSupplies(ro),
                    StationName = ro.ReliefStation?.Name,
                    Status = ro.Status.ToString(),
                    StartedAt = ro.StartedAt,
                    EndedAt = ro.EndedAt
                }).ToList(),
                Verifications = request.Verifications.Select(v => new RequestVerificationDto
                {
                    RequestVerificationId = v.RequestVerificationId,
                    Status = v.Status,
                    Method = v.Method,
                    Note = v.Note,
                    Reason = v.Reason,
                    VerifiedBy = v.VerifiedBy,
                    VerifiedAt = v.VerifiedAt
                }).ToList(),
                AssignedRescueTeam = BuildAssignedRescueTeamDto(request),
                Supplies = request.RescueOperations.SelectMany(MapOperationSupplies).ToList()
            };
        }

        private AssignedRescueTeamDto? BuildAssignedRescueTeamDto(RescueRequest request)
        {
            var activeOperation = request.RescueOperations
                .Where(o => o.TeamId.HasValue
                    && o.Team != null
                    && o.Status != RescueOperationStatus.Cancelled
                    && o.Status != RescueOperationStatus.Closed)
                .OrderByDescending(o => o.StartedAt)
                .FirstOrDefault();

            if (activeOperation == null || !activeOperation.TeamId.HasValue || activeOperation.Team == null)
            {
                return null;
            }

            var latestTracking = activeOperation.Team.TrackingPoints
                .OrderByDescending(t => t.CapturedAtUtc)
                .FirstOrDefault();

            var activeBatch = activeOperation.Team.RescueBatches
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefault();

            var batchItem = activeBatch?.Items.FirstOrDefault(i => i.RescueRequestId == request.RequestId);

            return new AssignedRescueTeamDto
            {
                RescueOperationId = activeOperation.RescueOperationId,
                TeamId = activeOperation.TeamId.Value,
                TeamName = activeOperation.Team.Name,
                VehicleId = activeOperation.VehicleId,
                VehicleName = activeOperation.Vehicle?.VehicleType?.TypeName,
                VehicleLicensePlate = activeOperation.Vehicle?.LicensePlate,
                Vehicles = MapAssignedVehicles(activeOperation),
                Supplies = MapOperationSupplies(activeOperation),
                OperationStatus = activeOperation.Status.ToString(),
                CurrentLatitude = latestTracking?.Latitude,
                CurrentLongitude = latestTracking?.Longitude,
                LastTrackedAt = latestTracking?.CapturedAtUtc,
                EstimatedMinutesToArrival = batchItem?.EstimatedMinutes,
                DistanceKmToVictim = batchItem?.DistanceKm,
                RoutePolyline = activeBatch?.RoutePolyline,
                TotalDistanceKm = activeBatch?.TotalDistanceKm,
                TotalEstimatedMinutes = activeBatch?.EstimatedMinutes
            };
        }

        private async Task<RescueRouteSnapshot> BuildRouteSnapshotAsync(
            double originLat,
            double originLng,
            double destinationLat,
            double destinationLng,
            CancellationToken cancellationToken)
        {
            try
            {
                var matrix = await _goongDistanceService.GetDistanceMatrixAsync(
                    originLat,
                    originLng,
                    new List<(double lat, double lng)> { (destinationLat, destinationLng) },
                    cancellationToken: cancellationToken);

                var element = matrix.Elements.FirstOrDefault();
                var hasValidDistance = element != null
                    && string.Equals(element.Status, "OK", StringComparison.OrdinalIgnoreCase)
                    && element.DistanceMeters.HasValue;

                if (hasValidDistance)
                {
                    return new RescueRouteSnapshot
                    {
                        DistanceMeters = element!.DistanceMeters,
                        DistanceKm = element.DistanceMeters!.Value / 1000d,
                        DurationSeconds = element.DurationSeconds,
                        DurationMinutes = element.DurationSeconds.HasValue
                            ? Math.Max(1, (int)Math.Ceiling(element.DurationSeconds.Value / 60d))
                            : null
                    };
                }
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
            }

            var fallbackKm = CalculateDistance(originLat, originLng, destinationLat, destinationLng);
            var fallbackMinutes = Math.Max(1, (int)Math.Ceiling((fallbackKm / 30d) * 60d));

            return new RescueRouteSnapshot
            {
                DistanceKm = fallbackKm,
                DistanceMeters = (int)Math.Round(fallbackKm * 1000d),
                DurationMinutes = fallbackMinutes,
                DurationSeconds = fallbackMinutes * 60
            };
        }

        private sealed class RescueRouteSnapshot
        {
            public double? DistanceKm { get; set; }
            public int? DurationMinutes { get; set; }
            public int? DistanceMeters { get; set; }
            public int? DurationSeconds { get; set; }
        }

        private static RescueBatchQueueResponseDto MapToBatchQueueResponseDto(RescueBatch batch)
        {
            return new RescueBatchQueueResponseDto
            {
                RescueBatchId = batch.RescueBatchId,
                TeamId = batch.TeamId,
                IsActive = batch.IsActive,
                Status = batch.Status,
                RoutePolyline = batch.RoutePolyline,
                TotalDistanceKm = batch.TotalDistanceKm,
                EstimatedMinutes = batch.EstimatedMinutes,
                CreatedAt = batch.CreatedAt,
                ClosedAt = batch.ClosedAt,
                Items = batch.Items
                    .OrderBy(i => i.SequenceOrder)
                    .Select(i => new RescueBatchQueueItemDto
                    {
                        RescueBatchItemId = i.RescueBatchItemId,
                        RescueRequestId = i.RescueRequestId,
                        DisasterType = i.RescueRequest?.DisasterType.ToString(),
                        RescueRequestType = i.RescueRequest?.RescueRequestType.ToString(),
                        RescueRequestStatus = i.RescueRequest?.RescueRequestStatus.ToString(),
                        Description = i.RescueRequest?.Description,
                        Address = i.RescueRequest?.Address,
                        Latitude = i.RescueRequest?.Latitude,
                        Longitude = i.RescueRequest?.Longitude,
                        ReporterFullName = i.RescueRequest?.ReporterFullName,
                        ReporterPhone = i.RescueRequest?.ReporterPhone,
                        PriorityPoint = i.RescueRequest.PriorityPoint,
                        PriorityLevel = i.RescueRequest.RescuePriorityLevel,
                        SequenceOrder = i.SequenceOrder,
                        IsAutoAssigned = i.IsAutoAssigned,
                        DistanceKm = i.DistanceKm,
                        EstimatedMinutes = i.EstimatedMinutes,
                        VehicleId = i.RescueRequest?.RescueOperations?
                            .Where(ro => ro.TeamId == batch.TeamId)
                            .OrderByDescending(ro => ro.StartedAt)
                            .Select(ro => ro.VehicleId)
                            .FirstOrDefault(),
                        VehicleName = i.RescueRequest?.RescueOperations?
                            .Where(ro => ro.TeamId == batch.TeamId)
                            .OrderByDescending(ro => ro.StartedAt)
                            .Select(ro => ro.Vehicle != null ? ro.Vehicle.VehicleType != null ? ro.Vehicle.VehicleType.TypeName : null : null)
                            .FirstOrDefault(),
                        VehicleLicensePlate = i.RescueRequest?.RescueOperations?
                            .Where(ro => ro.TeamId == batch.TeamId)
                            .OrderByDescending(ro => ro.StartedAt)
                            .Select(ro => ro.Vehicle != null ? ro.Vehicle.LicensePlate : null)
                            .FirstOrDefault(),
                        Vehicles = i.RescueRequest?.RescueOperations?
                            .Where(ro => ro.TeamId == batch.TeamId)
                            .OrderByDescending(ro => ro.StartedAt)
                            .Select(ro => MapAssignedVehicles(ro))
                            .FirstOrDefault() ?? new List<AssignedVehicleDto>(),
                        Supplies = i.RescueRequest?.RescueOperations?
                            .Where(ro => ro.TeamId == batch.TeamId)
                            .OrderByDescending(ro => ro.StartedAt)
                            .Select(ro => MapOperationSupplies(ro))
                            .FirstOrDefault() ?? new List<RescueOperationSupplyDto>(),
                        Status = i.Status,
                        CreatedAt = i.CreatedAt
                    })
                    .ToList()
            };
        }

        private async Task RecalculateBatchEtaFromLatestTrackingAsync(
            Guid teamId,
            CancellationToken cancellationToken)
        {
            var activeBatch = await _unitOfWork.RescueBatches.GetActiveByTeamIdAsync(teamId, cancellationToken);
            if (activeBatch == null)
                return;

            var trackingPoint = await _unitOfWork.TeamTrackingPoints.GetLatestPointAsync(teamId, cancellationToken);
            if (trackingPoint == null)
                return;

            var candidateItems = activeBatch.Items
                .Where(i => i.Status == RescueBatchItemStatus.Pending || i.Status == RescueBatchItemStatus.InProgress)
                .OrderBy(i => i.SequenceOrder)
                .ToList();

            if (!candidateItems.Any())
                return;

            var requestIds = candidateItems.Select(i => i.RescueRequestId).Distinct().ToHashSet();
            var requests = await _unitOfWork.RescueRequests.GetAllAsync(cancellationToken);
            var requestMap = requests
                .Where(r => requestIds.Contains(r.RequestId))
                .ToDictionary(r => r.RequestId, r => r);

            var mappedItems = new List<RescueBatchItem>();
            var destinations = new List<(double lat, double lng)>();

            foreach (var item in candidateItems)
            {
                if (!requestMap.TryGetValue(item.RescueRequestId, out var rr))
                    continue;

                mappedItems.Add(item);
                destinations.Add((rr.Latitude, rr.Longitude));
            }

            if (!mappedItems.Any())
                return;

            try
            {
                var matrix = await _goongDistanceService.GetDistanceMatrixAsync(
                    trackingPoint.Latitude,
                    trackingPoint.Longitude,
                    destinations,
                    cancellationToken: cancellationToken);

                for (var i = 0; i < mappedItems.Count; i++)
                {
                    var item = mappedItems[i];
                    var element = i < matrix.Elements.Count ? matrix.Elements[i] : null;

                    var hasValidDistance = element != null
                        && string.Equals(element.Status, "OK", StringComparison.OrdinalIgnoreCase)
                        && element.DistanceMeters.HasValue;

                    if (hasValidDistance)
                    {
                        item.DistanceKm = element!.DistanceMeters!.Value / 1000d;
                        item.EstimatedMinutes = element.DurationSeconds.HasValue
                            ? Math.Max(1, (int)Math.Ceiling(element.DurationSeconds.Value / 60d))
                            : null;
                    }
                    else if (requestMap.TryGetValue(item.RescueRequestId, out var req))
                    {
                        var fallbackKm = CalculateDistance(
                            trackingPoint.Latitude,
                            trackingPoint.Longitude,
                            req.Latitude,
                            req.Longitude);

                        item.DistanceKm = fallbackKm;
                        item.EstimatedMinutes = Math.Max(1, (int)Math.Ceiling((fallbackKm / 30d) * 60d));
                    }
                }
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                foreach (var item in mappedItems)
                {
                    if (!requestMap.TryGetValue(item.RescueRequestId, out var req))
                        continue;

                    var fallbackKm = CalculateDistance(
                        trackingPoint.Latitude,
                        trackingPoint.Longitude,
                        req.Latitude,
                        req.Longitude);

                    item.DistanceKm = fallbackKm;
                    item.EstimatedMinutes = Math.Max(1, (int)Math.Ceiling((fallbackKm / 30d) * 60d));
                }
            }
        }


        // ─── Extended API Implementations ──────────────────────────────────────

        public async Task<PaginatedRescueRequestResponseDto> GetMyRequestsAsync(
            Guid userId,
            MyRescueRequestQueryDto query,
            CancellationToken cancellationToken = default)
        {
            query ??= new MyRescueRequestQueryDto();
            var pageNumber = query.PageNumber <= 0 ? 1 : query.PageNumber;
            var pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

            var (items, totalCount) = await _unitOfWork.RescueRequests.GetByReporterUserIdAsync(
                userId, pageNumber, pageSize, query.StatusFilter, cancellationToken);

            return new PaginatedRescueRequestResponseDto
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = items.Select(r => MapToResponseDto(r)).ToList()
            };
        }

        public async Task<RescueRequestResponseDto> CancelRescueRequestAsync(
            Guid requestId,
            Guid userId,
            CancelRescueRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            var request = await _unitOfWork.RescueRequests.GetByIdForCancellationUpdateAsync(requestId, cancellationToken);
            if (request == null)
                throw new InvalidOperationException($"Rescue request {requestId} not found.");

            if (request.ReporterUserId == null || request.ReporterUserId.Value != userId)
                throw new UnauthorizedAccessException("Chi chu cua yeu cau moi duoc huy.");

            if (request.RescueRequestStatus != RescueRequestStatus.Pending)
                throw new InvalidOperationException(
                    $"Khong the huy yeu cau o trang thai '{request.RescueRequestStatus}'. Chi huy duoc khi dang Pending.");

            if (string.IsNullOrWhiteSpace(dto?.Reason))
                throw new InvalidOperationException("Vui long cung cap ly do huy.");

            await _unitOfWork.RescueRequests.DetachTrackedAttachmentsAsync(request.RequestId, cancellationToken);

            var now = DateTime.UtcNow;

            request.RescueRequestStatus = RescueRequestStatus.Cancelled;
            request.UpdatedAt = now;

            var cancelVerification = new RequestVerification
            {
                RequestVerificationId = Guid.NewGuid(),
                RequestId = request.RequestId,
                Status = RequestVerificationStatus.Rejected,
                Method = VerificationMethod.None,
                Reason = "Hủy bởi người gửi",
                Note = string.IsNullOrWhiteSpace(dto.Reason)
                    ? "Người dân tự hủy yêu cầu."
                    : $"Người dân tự hủy yêu cầu. Lý do: {dto.Reason}",
                VerifiedBy = userId,
                VerifiedAt = now
            };

            await _unitOfWork.RequestVerifications.AddAsync(cancelVerification);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await GetRescueRequestByIdAsync(requestId, cancellationToken);
        }

        private async Task EnsureRescueTeamTypeAsync(Guid teamId, CancellationToken cancellationToken)
        {
            var team = await _unitOfWork.Teams.GetByIdAsync(teamId);
            if (team == null)
                throw new InvalidOperationException($"Team '{teamId}' was not found.");

            if (team.Status != TeamStatus.Active)
                throw new InvalidOperationException("Team is not active.");

            if (team.TeamType != TeamType.Rescue)
                throw new InvalidOperationException("Chỉ team cứu hộ mới được tham gia luồng cứu hộ.");
        }

        public async Task<TeamLocationForRequestDto?> GetTeamLocationForRequestAsync(
            Guid requestId,
            CancellationToken cancellationToken = default)
        {
            var request = await _unitOfWork.RescueRequests.GetByIdAsync(requestId, cancellationToken);
            if (request == null)
                throw new InvalidOperationException($"Rescue request {requestId} not found.");

            var activeOperation = request.RescueOperations
                .Where(o => o.TeamId.HasValue
                    && o.Status != RescueOperationStatus.Cancelled
                    && o.Status != RescueOperationStatus.Closed)
                .OrderByDescending(o => o.StartedAt)
                .FirstOrDefault();

            if (activeOperation == null || !activeOperation.TeamId.HasValue)
                return null;

            var team = await _unitOfWork.Teams.GetByIdAsync(activeOperation.TeamId.Value);
            if (team == null)
                return null;

            var latestTracking = await _unitOfWork.TeamTrackingPoints.GetLatestPointAsync(
                activeOperation.TeamId.Value, cancellationToken);

            var activeBatch = await _unitOfWork.RescueBatches.GetActiveByTeamIdAsync(
                activeOperation.TeamId.Value, cancellationToken);

            var batchItem = activeBatch?.Items
                .FirstOrDefault(i => i.RescueRequestId == requestId);

            return new TeamLocationForRequestDto
            {
                RescueOperationId = activeOperation.RescueOperationId,
                TeamId = activeOperation.TeamId.Value,
                TeamName = team.Name,
                OperationStatus = activeOperation.Status.ToString(),
                CurrentLatitude = latestTracking?.Latitude,
                CurrentLongitude = latestTracking?.Longitude,
                LastTrackedAt = latestTracking?.CapturedAtUtc,
                EstimatedMinutesToArrival = batchItem?.EstimatedMinutes,
                DistanceKmToVictim = batchItem?.DistanceKm
            };
        }

        public async Task<RescueRequestStatsDto> GetRescueStatsAsync(CancellationToken cancellationToken = default)
        {
            var counts = await _unitOfWork.RescueRequests.GetStatusCountsAsync(cancellationToken);

            static int Get(Dictionary<int, int> d, RescueRequestStatus s)
                => d.TryGetValue((int)s, out var v) ? v : 0;

            var total = counts.Values.Sum();

            return new RescueRequestStatsDto
            {
                Total = total,
                Pending = Get(counts, RescueRequestStatus.Pending),
                Verified = Get(counts, RescueRequestStatus.Verified),
                Assigned = Get(counts, RescueRequestStatus.Assigned),
                InProgress = Get(counts, RescueRequestStatus.InProgress),
                Completed = Get(counts, RescueRequestStatus.Completed),
                Cancelled = Get(counts, RescueRequestStatus.Cancelled)
            };
        }

        public async Task<RescueTeamHistoryResponseDto> GetTeamRescueHistoryAsync(
            Guid teamId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var (batches, totalCount) = await _unitOfWork.RescueBatches.GetCompletedByTeamIdAsync(
                teamId, pageNumber, pageSize, cancellationToken);

            var requestIds = batches
                .SelectMany(b => b.Items.Select(i => i.RescueRequestId))
                .Distinct()
                .ToHashSet();

            var allRequests = await _unitOfWork.RescueRequests.GetAllAsync(cancellationToken);
            var requestMap = allRequests
                .Where(r => requestIds.Contains(r.RequestId))
                .ToDictionary(r => r.RequestId);

            var data = batches.Select(b => new RescueBatchHistoryItemDto
            {
                RescueBatchId = b.RescueBatchId,
                CreatedAt = b.CreatedAt,
                ClosedAt = b.ClosedAt,
                TotalRequests = b.Items.Count,
                CompletedRequests = b.Items.Count(i => i.Status == RescueBatchItemStatus.Done),
                Requests = b.Items
                    .OrderBy(i => i.SequenceOrder)
                    .Select(i =>
                    {
                        requestMap.TryGetValue(i.RescueRequestId, out var rr);
                        var teamOperation = rr?.RescueOperations?
                            .Where(ro => ro.TeamId == teamId)
                            .OrderByDescending(ro => ro.StartedAt)
                            .FirstOrDefault();

                        return new RescueCompletedRequestSummaryDto
                        {
                            RequestId = i.RescueRequestId,
                            VehicleId = teamOperation?.VehicleId,
                            VehicleName = teamOperation?.Vehicle?.VehicleType?.TypeName,
                            VehicleLicensePlate = teamOperation?.Vehicle?.LicensePlate,
                            Vehicles = teamOperation != null ? MapAssignedVehicles(teamOperation) : new List<AssignedVehicleDto>(),
                            Address = rr?.Address,
                            DisasterType = rr?.DisasterType.ToString() ?? "-",
                            RescueRequestType = rr?.RescueRequestType.ToString(),
                            Priority = rr?.PriorityPoint,
                            PriorityLevel = rr?.RescuePriorityLevel.ToString(),
                            RescueRequestStatus = rr?.RescueRequestStatus.ToString() ?? "-",
                            ReporterFullName = rr?.ReporterFullName ?? "-",
                            ReporterPhone = rr?.ReporterPhone ?? "-",
                            CreatedAt = rr?.CreatedAt ?? b.CreatedAt,
                            UpdatedAt = rr?.UpdatedAt,
                            SequenceOrder = i.SequenceOrder,
                            BatchItemStatus = i.Status.ToString()
                        };
                    }).ToList()
            }).ToList();

            return new RescueTeamHistoryResponseDto
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = data
            };
        }
        private sealed class StationDistanceScore
        {
            public required ReliefStation Station { get; set; }
            public double DistanceKm { get; set; }
        }

        private static List<AssignedVehicleDto> MapAssignedVehicles(RescueOperation operation)
        {
            var vehicles = operation.RescueOperationVehicles
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.AssignedAt)
                .Select(x => new AssignedVehicleDto
                {
                    VehicleId = x.VehicleId,
                    VehicleName = x.Vehicle?.VehicleType?.TypeName,
                    VehicleLicensePlate = x.Vehicle?.LicensePlate,
                    IsPrimary = x.IsPrimary
                })
                .ToList();

            if (vehicles.Count == 0 && operation.VehicleId.HasValue)
            {
                vehicles.Add(new AssignedVehicleDto
                {
                    VehicleId = operation.VehicleId.Value,
                    VehicleName = operation.Vehicle?.VehicleType?.TypeName,
                    VehicleLicensePlate = operation.Vehicle?.LicensePlate,
                    IsPrimary = true
                });
            }

            return vehicles;
        }

        private static List<AssignedVehicleDto> GetAssignedVehicles(RescueOperation operation)
            => MapAssignedVehicles(operation);

        private static List<RescueOperationSupplyDto> MapOperationSupplies(RescueOperation operation)
        {
            return operation.RescueOperationSupplies
                .OrderBy(x => x.CreatedAt)
                .Select(x => new RescueOperationSupplyDto
                {
                    RescueOperationSupplyId = x.RescueOperationSupplyId,
                    RescueOperationId = x.RescueOperationId,
                    SourceInventoryId = x.SourceInventoryId,
                    SourceInventoryName = x.SourceInventory?.ReliefStation?.Name,
                    SupplyItemId = x.SupplyItemId,
                    SupplyItemName = x.SupplyItem?.Name,
                    Quantity = x.Quantity,
                    Unit = x.Unit,
                    Notes = x.Notes,
                    InventoryTransactionId = x.InventoryTransactionId,
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy
                })
                .ToList();
        }

        private async Task ReplaceRescueOperationVehiclesAsync(
            RescueOperation operation,
            List<Guid> vehicleIds,
            string? note,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var normalizedVehicleIds = vehicleIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            var existingNormalizedIds = await _unitOfWork.RescueOperationVehicles
                .GetByOperationIdAsync(operation.RescueOperationId, cancellationToken);

            var existingIds = existingNormalizedIds
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.AssignedAt)
                .Select(x => x.VehicleId)
                .Distinct()
                .ToList();

            if (existingIds.SequenceEqual(normalizedVehicleIds))
            {
                return;
            }

            var replacement = new List<RescueOperationVehicle>();
            for (var i = 0; i < normalizedVehicleIds.Count; i++)
            {
                replacement.Add(new RescueOperationVehicle
                {
                    RescueOperationVehicleId = Guid.NewGuid(),
                    RescueOperationId = operation.RescueOperationId,
                    VehicleId = normalizedVehicleIds[i],
                    IsPrimary = i == 0,
                    AssignedAt = now,
                    Note = note
                });
            }

            await _unitOfWork.RescueOperationVehicles.ReplaceForOperationAsync(operation.RescueOperationId, replacement, cancellationToken);

            operation.RescueOperationVehicles = replacement;
        }

        private async Task SyncRescueOperationSuppliesAsync(
            RescueOperation operation,
            List<AssignRescueSupplyItemDto>? supplies,
            string? note,
            CancellationToken cancellationToken)
        {
            if (supplies == null || supplies.Count == 0)
            {
                return;
            }

            var normalizedSupplies = supplies
                .Where(x => x.SourceInventoryId != Guid.Empty && x.SupplyItemId != Guid.Empty && x.Quantity > 0)
                .GroupBy(x => new { x.SourceInventoryId, x.SupplyItemId, x.Unit })
                .Select(g => new AssignRescueSupplyItemDto
                {
                    SourceInventoryId = g.Key.SourceInventoryId,
                    SupplyItemId = g.Key.SupplyItemId,
                    Quantity = g.Sum(x => x.Quantity),
                    Unit = g.Key.Unit,
                    Notes = string.Join("; ", g.Select(x => x.Notes).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
                })
                .ToList();

            if (normalizedSupplies.Count == 0)
            {
                return;
            }

            var incomingKeys = normalizedSupplies
                .Select(x => (x.SourceInventoryId, x.SupplyItemId, Unit: x.Unit ?? string.Empty))
                .ToHashSet();

            var existingSupplies = operation.RescueOperationSupplies.ToList();
            foreach (var existing in existingSupplies)
            {
                var existingKey = (existing.SourceInventoryId, existing.SupplyItemId, Unit: existing.Unit ?? string.Empty);
                if (!incomingKeys.Contains(existingKey))
                {
                    operation.RescueOperationSupplies.Remove(existing);
                }
            }

            var groupedByInventory = normalizedSupplies.GroupBy(x => x.SourceInventoryId).ToList();
            var transactionIdByInventory = new Dictionary<Guid, Guid>();

            foreach (var inventoryGroup in groupedByInventory)
            {
                var tx = await _inventoryTransactionService.CreateTransactionAsync(new CreateTransactionRequest
                {
                    InventoryId = inventoryGroup.Key,
                    Type = TransactionType.Export,
                    Reason = TransactionReason.Other,
                    Notes = note,
                    SourceReference = operation.RescueOperationId.ToString(),
                    Items = inventoryGroup.Select(x => new TransactionItemRequest
                    {
                        SupplyItemId = x.SupplyItemId,
                        Quantity = x.Quantity,
                        Notes = x.Notes
                    }).ToList()
                }, autoSave: false, cancellationToken: cancellationToken);

                transactionIdByInventory[inventoryGroup.Key] = tx.TransactionId;
            }

            var inventoryIds = groupedByInventory.Select(g => g.Key).ToList();
            var supplyItemIds = normalizedSupplies.Select(x => x.SupplyItemId).Distinct().ToList();

            var inventories = await _unitOfWork.Inventories.GetQueryable()
                .Where(i => inventoryIds.Contains(i.InventoryId))
                .ToDictionaryAsync(i => i.InventoryId, cancellationToken);

            var supplyItems = await _unitOfWork.SupplyItems.GetQueryable()
                .Where(s => supplyItemIds.Contains(s.SupplyItemId))
                .ToDictionaryAsync(s => s.SupplyItemId, cancellationToken);

            var now = DateTime.UtcNow;
            foreach (var item in normalizedSupplies)
            {
                var existing = operation.RescueOperationSupplies.FirstOrDefault(x =>
                    x.SourceInventoryId == item.SourceInventoryId &&
                    x.SupplyItemId == item.SupplyItemId &&
                    (x.Unit ?? string.Empty) == (item.Unit ?? string.Empty));

                if (existing != null)
                {
                    existing.Quantity = item.Quantity;
                    existing.Unit = item.Unit ?? supplyItems[item.SupplyItemId].Unit;
                    existing.Notes = item.Notes;
                    existing.InventoryTransactionId = transactionIdByInventory[item.SourceInventoryId];
                    existing.SourceInventory = inventories[item.SourceInventoryId];
                    existing.SupplyItem = supplyItems[item.SupplyItemId];
                }
                else
                {
                    operation.RescueOperationSupplies.Add(new RescueOperationSupply
                    {
                        RescueOperationSupplyId = Guid.NewGuid(),
                        RescueOperationId = operation.RescueOperationId,
                        SourceInventoryId = item.SourceInventoryId,
                        SupplyItemId = item.SupplyItemId,
                        Quantity = item.Quantity,
                        Unit = item.Unit ?? supplyItems[item.SupplyItemId].Unit,
                        Notes = item.Notes,
                        CreatedAt = now,
                        CreatedBy = _currentUserService.UserId,
                        InventoryTransactionId = transactionIdByInventory[item.SourceInventoryId],
                        SourceInventory = inventories[item.SourceInventoryId],
                        SupplyItem = supplyItems[item.SupplyItemId]
                    });
                }
            }
        }
    }
}



