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

        public RescueRequestService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        /// <summary>Gửi yêu cầu cứu hộ mới</summary>
        public async Task<RescueRequestResponseDto> CreateRescueRequestAsync(
            CreateRescueRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // 1. Lấy thông tin người dùng hiện tại (nếu có)
            var currentUserId = _currentUserService.UserId;
            Domain.Entities.ApplicationUser? currentUser = null;

            if (currentUserId.HasValue)
            {
                currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId.Value);
            }

            // 2. Tạo RescueRequest entity
            var rescueRequest = new Domain.Entities.RescueRequest
            {
                RequestId = Guid.NewGuid(),
                RequestType = Domain.Enum.RequestType.Rescue,
                DisasterType = (Domain.Enum.DisasterType)request.DisasterType,
                Description = request.Description,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Accuracy = request.Accuracy,
                Address = request.Address ?? string.Empty,
                LocationId = request.LocationId,
                Note = request.Note,
                ReporterUserId = currentUserId, // can be null for anonymous reports
                ReporterFullName = currentUser?.UserName ?? request.ReporterFullName ?? "Anonymous",
                ReporterPhone = currentUser?.PhoneNumber ?? request.ReporterPhone ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
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

            var verification = new RequestVerification
            {
                RequestVerificationId = Guid.NewGuid(),
                RequestId = rescueRequest.RequestId,
                Status = RequestVerificationStatus.Pending,
                Method = VerificationMethod.None
            };

            rescueRequest.Verifications.Add(verification);


            // 4. Lưu RescueRequest vào database (before priorities so RescueRequestId exists)
            await _unitOfWork.RescueRequests.AddAsync(rescueRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. Nếu user gửi Normal request và có SelectedPriorityCriteriaIds -> tính điểm từ các mục user chọn
            if (request.RescueType == RescueRequestType.Normal && request.SelectedPriorityCriteriaIds != null && request.SelectedPriorityCriteriaIds.Count > 0)
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
            else
            {
                // 6. Fallback: tự động tính priority dựa trên nội dung / attachment nếu user không chọn criteria
                //await CalculatePriorityAsync(rescueRequest.RequestId, cancellationToken);
            }

            // 7. Nếu loại Emergency thì bypass xác minh và dispatch ngay
            if (request.RescueType == RescueRequestType.Emergency) // Emergency
            {
                // Recalculate priority if user provided selections even for emergency (optional)
                if (request.SelectedPriorityCriteriaIds != null && request.SelectedPriorityCriteriaIds.Count > 0)
                {
                    // ensure priority already calculated above; if not, calculate now
                    if (!rescueRequest.PriorityPoint.HasValue)
                    {
                        //await CalculatePriorityAsync(rescueRequest.RequestId, cancellationToken);
                    }
                }

                rescueRequest.RescueRequestStatus = Domain.Enum.RescueRequestStatus.Verified;
                await _unitOfWork.RescueRequests.UpdateAsync(rescueRequest);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Dispatch ngay
                await DispatchToStationsAsync(rescueRequest.RequestId, cancellationToken);
            }

            // 8. Trả về response
            return await GetRescueRequestByIdAsync(rescueRequest.RequestId, cancellationToken);
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

        public async Task<RescueRequestResponseDto> VerifyRescueRequestAsync(
            Guid requestId,
            VerifyRescueRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var rescueRequest = await _unitOfWork.RescueRequests.GetByIdAsync(requestId, cancellationToken);

            if (rescueRequest == null)
                throw new InvalidOperationException($"Rescue request {requestId} not found");

            if (request.Status == RequestVerificationStatus.Approved) // Approved
            {
                rescueRequest.RescueRequestStatus = Domain.Enum.RescueRequestStatus.Verified;

                var verification = rescueRequest.Verifications
                    .FirstOrDefault(v => v.Status == RequestVerificationStatus.Pending);

                if (verification == null)
                    throw new InvalidOperationException("No pending verification found");

                verification.Status = request.Status;
                verification.Note = request.Note;
                verification.Method = request.Method;
                verification.VerifiedAt = DateTime.UtcNow;
                verification.VerifiedBy = _currentUserService.UserId;

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await DispatchToStationsAsync(requestId, cancellationToken);
            }
            else if (request.Status == RequestVerificationStatus.Rejected) // Rejected
            {
                rescueRequest.RescueRequestStatus = Domain.Enum.RescueRequestStatus.Cancelled;

                var verification = new RequestVerification
                {
                    RequestVerificationId = Guid.NewGuid(),
                    RequestId = requestId,
                    VerifiedAt = DateTime.UtcNow,
                    Reason = request.Reason,
                    Status = RequestVerificationStatus.Rejected,
                    VerifiedBy = (Guid)_currentUserService.UserId,
                };

                rescueRequest.Verifications.Add(verification);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return await GetRescueRequestByIdAsync(requestId, cancellationToken);
        }

        //public async Task<int> CalculatePriorityAsync(
        //    Guid requestId,
        //    CancellationToken cancellationToken = default)
        //{
        //    var rescueRequest = await _unitOfWork.RescueRequests.GetByIdAsync(requestId, cancellationToken);

        //    if (rescueRequest == null)
        //        throw new InvalidOperationException($"Rescue request {requestId} not found");

        //    var priorityCriterias = await _unitOfWork.PriorityCriterias.GetByDisasterTypeAsync(
        //        rescueRequest.DisasterType, cancellationToken);

        //    int totalPoints = 0;

        //    var attachmentCount = rescueRequest.Attachments.Count;
        //    var attachmentCriteria = priorityCriterias.FirstOrDefault(p => p.Code == "ATTACHMENT_COUNT");
        //    if (attachmentCriteria != null && attachmentCount > 0)
        //    {
        //        int attachmentPoints = Math.Min(attachmentCount * (attachmentCriteria.Point / 5), attachmentCriteria.Point);
        //        totalPoints += attachmentPoints;

        //        var requestPriority = new RescueRequestPriority
        //        {
        //            RescueRequestId = requestId,
        //            PriorityCriteriaId = attachmentCriteria.PriorityCriteriaId,
        //            AppliedPoint = attachmentPoints,
        //            Status = "Applied"
        //        };

        //        await _unitOfWork.RescueRequestPriorities.AddAsync(requestPriority);
        //    }

        //    var disasterCriteria = priorityCriterias.FirstOrDefault(p => p.Code == "DISASTER_SEVERITY");
        //    if (disasterCriteria != null)
        //    {
        //        totalPoints += disasterCriteria.Point;

        //        var requestPriority = new RescueRequestPriority
        //        {
        //            RescueRequestId = requestId,
        //            PriorityCriteriaId = disasterCriteria.PriorityCriteriaId,
        //            AppliedPoint = disasterCriteria.Point,
        //            Status = "Applied"
        //        };

        //        await _unitOfWork.RescueRequestPriorities.AddAsync(requestPriority);
        //    }

        //    var descriptionLength = rescueRequest.Description?.Length ?? 0;
        //    var descriptionCriteria = priorityCriterias.FirstOrDefault(p => p.Code == "DESCRIPTION_DETAIL");
        //    if (descriptionCriteria != null && descriptionLength > 100)
        //    {
        //        totalPoints += descriptionCriteria.Point;

        //        var requestPriority = new RescueRequestPriority
        //        {
        //            RescueRequestId = requestId,
        //            PriorityCriteriaId = descriptionCriteria.PriorityCriteriaId,
        //            AppliedPoint = descriptionCriteria.Point,
        //            Status = "Applied"
        //        };

        //        await _unitOfWork.RescueRequestPriorities.AddAsync(requestPriority);
        //    }

        //    int priorityLevel = CalculatePriorityLevel(totalPoints);
        //    rescueRequest.Priority = priorityLevel;

        //    await _unitOfWork.RescueRequests.UpdateAsync(rescueRequest);
        //    await _unitOfWork.SaveChangesAsync(cancellationToken);

        //    return priorityLevel;
        //}

        public async Task DispatchToStationsAsync(
            Guid requestId,
            CancellationToken cancellationToken = default)
        {
            var rescueRequest = await _unitOfWork.RescueRequests.GetByIdAsync(requestId, cancellationToken);

            if (rescueRequest == null)
                throw new InvalidOperationException($"Rescue request {requestId} not found");

            var priorityLevel = rescueRequest.RescuePriorityLevel;

            var allStations = await _unitOfWork.ReliefStations.GetAllAsync();
            var activeStations = allStations.Where(s => s.IsActive).ToList();

            if (!activeStations.Any())
                throw new InvalidOperationException("No active relief stations available");

            List<Domain.Entities.ReliefStation> targetStations = new();

            if (priorityLevel == RescuePriorityLevel.Low)
            {
                var nearestStation = GetNearestStation(activeStations, rescueRequest.Latitude, rescueRequest.Longitude);
                if (nearestStation != null)
                    targetStations.Add(nearestStation);

                rescueRequest.DispatchMode = Domain.Enum.DispatchMode.NearestStation;
            }
            else if (priorityLevel == RescuePriorityLevel.Medium)
            {
                var nearestStations = GetNearestStations(activeStations, rescueRequest.Latitude, rescueRequest.Longitude, 2);
                targetStations.AddRange(nearestStations);

                rescueRequest.DispatchMode = Domain.Enum.DispatchMode.MultipleStations;
            }
            else
            {
                if (rescueRequest.LocationId.HasValue)
                {
                    targetStations = activeStations.Where(s => s.LocationId == rescueRequest.LocationId.Value).ToList();
                }

                if (!targetStations.Any())
                    targetStations = activeStations;

                rescueRequest.DispatchMode = Domain.Enum.DispatchMode.ProvinceBroadcast;
            }

            rescueRequest.RescueRequestStatus = Domain.Enum.RescueRequestStatus.Assigned;

            foreach (var station in targetStations)
            {
                var operation = new RescueOperation
                {
                    RescueOperationId = Guid.NewGuid(),
                    RescueRequestId = requestId,
                    ReliefStationId = station.ReliefStationId,
                    StartedAt = DateTime.UtcNow,
                };

                await _unitOfWork.RescueOperations.AddAsync(operation);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateRescueRequestStatusAsync(
            Guid requestId,
            int newStatus,
            CancellationToken cancellationToken = default)
        {
            var rescueRequest = await _unitOfWork.RescueRequests.GetByIdAsync(requestId, cancellationToken);

            if (rescueRequest == null)
                throw new InvalidOperationException($"Rescue request {requestId} not found");

            rescueRequest.RescueRequestStatus = (Domain.Enum.RescueRequestStatus)newStatus;
            rescueRequest.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.RescueRequests.UpdateAsync(rescueRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private Domain.Entities.ReliefStation? GetNearestStation(
            List<Domain.Entities.ReliefStation> stations,
            double latitude,
            double longitude)
        {
            return stations
                .OrderBy(s => CalculateDistance(s.Latitude, s.Longitude, latitude, longitude))
                .FirstOrDefault();
        }

        private List<Domain.Entities.ReliefStation> GetNearestStations(
            List<Domain.Entities.ReliefStation> stations,
            double latitude,
            double longitude,
            int count)
        {
            return stations
                .OrderBy(s => CalculateDistance(s.Latitude, s.Longitude, latitude, longitude))
                .Take(count)
                .ToList();
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

        private RescueRequestResponseDto MapToResponseDto(Domain.Entities.RescueRequest request)
        {
            return new RescueRequestResponseDto
            {
                RequestId = request.RequestId,
                DisasterType = request.DisasterType.ToString(),
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
                }).ToList()
            };
        }
    }
}