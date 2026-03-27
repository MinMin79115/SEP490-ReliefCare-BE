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
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    public class RescueRequestService : IRescueRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IGoongDistanceService _goongDistanceService;
        private readonly IWeatherService _weatherService;

        public RescueRequestService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IGoongDistanceService goongDistanceService,
            IWeatherService weatherService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _goongDistanceService = goongDistanceService;
            _weatherService = weatherService;
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
                        UploadedAt = DateTime.UtcNow
                    });
                }
            }

            var verificationStatus = RequestVerificationStatus.Pending;
            var verificationMethod = VerificationMethod.None;
            string? verificationNote = null;

            if (request.RescueType == RescueRequestType.Emergency)
            {
                verificationMethod = VerificationMethod.SystemAutoCheck;

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

                    verificationStatus = weather.WeatherRiskScore >= 40
                        ? RequestVerificationStatus.Approved
                        : RequestVerificationStatus.Pending;
                }
                catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
                {
                    verificationStatus = RequestVerificationStatus.Pending;
                    verificationMethod = VerificationMethod.SystemAutoCheck;

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
            return await GetRescueRequestByIdAsync(requestId, cancellationToken);
        }

        public async Task<RescueRequestResponseDto> AssignTeamToRescueAsync(
            Guid requestId,
            AssignRescueTeamRequestDto dto,
            CancellationToken cancellationToken = default)
        {
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

            stationOperation.TeamId = dto.TeamId;
            stationOperation.Status = RescueOperationStatus.Assigned;
            stationOperation.Note = dto.Note;

            request.RescueRequestStatus = RescueRequestStatus.InProgress;
            request.UpdatedAt = DateTime.UtcNow;

            await EnsureActiveBatchAndAppendRequestAsync(dto.TeamId, request.RequestId, cancellationToken);
            await RecalculateBatchEtaFromLatestTrackingAsync(dto.TeamId, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await GetRescueRequestByIdAsync(requestId, cancellationToken);
        }

        public async Task<BulkAssignRescueTeamResponseDto> AssignTeamToMultipleRescueRequestsAsync(
            AssignRescueTeamBulkRequestDto dto,
            CancellationToken cancellationToken = default)
        {
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
            var request = await _unitOfWork.RescueRequests.GetByIdAsync(requestId, cancellationToken);
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

            var team = await _unitOfWork.Teams.GetByIdAsync(operation.TeamId.Value);
            if (team == null)
                throw new InvalidOperationException("Team được gán cho operation không tồn tại.");

            if (!team.LeaderId.HasValue || team.LeaderId.Value != currentUserId.Value)
                throw new UnauthorizedAccessException("Chỉ team leader của operation mới được xác nhận hoàn tất cứu hộ.");

            if (dto?.Attachments == null || dto.Attachments.Count == 0)
                throw new InvalidOperationException("Completing rescue requires at least one image evidence.");

            var now = DateTime.UtcNow;

            foreach (var attachment in dto.Attachments)
            {
                request.Attachments.Add(new Attachment
                {
                    AttachmentId = Guid.NewGuid(),
                    RequestId = request.RequestId,
                    FileUrl = attachment.FileUrl,
                    ContentType = attachment.ContentType,
                    UploadedAt = now
                });
            }

            operation.Status = RescueOperationStatus.RescueCompleted;
            operation.EndedAt = now;

            if (!string.IsNullOrWhiteSpace(dto.Note))
            {
                operation.Note = string.IsNullOrWhiteSpace(operation.Note)
                    ? dto.Note
                    : $"{operation.Note}{Environment.NewLine}{dto.Note}";
            }

            await CompleteBatchItemAndAdvanceQueueAsync(
                operation.TeamId.Value,
                request.RequestId,
                now,
                cancellationToken);

            if (request.RescueOperations.All(o =>
                    o.Status == RescueOperationStatus.Closed ||
                    o.Status == RescueOperationStatus.Cancelled))
            {
                request.RescueRequestStatus = RescueRequestStatus.Completed;
            }

            request.UpdatedAt = now;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
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

                var nextRequest = await _unitOfWork.RescueRequests.GetByIdAsync(nextPending.RescueRequestId, cancellationToken);
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
            return await GetRescueRequestByIdAsync(requestId, cancellationToken);
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
                    StationName = ro.ReliefStation?.Name,
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
                    }).ToList()
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
                        SequenceOrder = i.SequenceOrder,
                        IsAutoAssigned = i.IsAutoAssigned,
                        DistanceKm = i.DistanceKm,
                        EstimatedMinutes = i.EstimatedMinutes,
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
            var request = await _unitOfWork.RescueRequests.GetByIdAsync(requestId, cancellationToken);
            if (request == null)
                throw new InvalidOperationException($"Rescue request {requestId} not found.");

            if (request.ReporterUserId == null || request.ReporterUserId.Value != userId)
                throw new UnauthorizedAccessException("Chi chu cua yeu cau moi duoc huy.");

            if (request.RescueRequestStatus != RescueRequestStatus.Pending)
                throw new InvalidOperationException(
                    $"Khong the huy yeu cau o trang thai '{request.RescueRequestStatus}'. Chi huy duoc khi dang Pending.");

            if (string.IsNullOrWhiteSpace(dto?.Reason))
                throw new InvalidOperationException("Vui long cung cap ly do huy.");

            request.RescueRequestStatus = RescueRequestStatus.Cancelled;
            request.UpdatedAt = DateTime.UtcNow;

            var cancelVerification = new RequestVerification
            {
                RequestVerificationId = Guid.NewGuid(),
                RequestId = request.RequestId,
                Status = RequestVerificationStatus.Rejected,
                Method = VerificationMethod.None,
                Reason = dto.Reason,
                Note = "Nguoi dan tu huy yeu cau.",
                VerifiedBy = userId,
                VerifiedAt = DateTime.UtcNow
            };
            request.Verifications.Add(cancelVerification);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await GetRescueRequestByIdAsync(requestId, cancellationToken);
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
                        return new RescueCompletedRequestSummaryDto
                        {
                            RequestId = i.RescueRequestId,
                            Address = rr?.Address,
                            DisasterType = rr?.DisasterType.ToString() ?? "-",
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
    }
}



