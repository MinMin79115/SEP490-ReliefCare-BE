using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.ReliefRequest.DTOs.Request;
using ReliefManagementSystem.Application.Features.ReliefRequest.DTOs.Response;
using ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    public class ReliefRequestService : IReliefRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ReliefRequestService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ReliefRequestResponseDto> CreateReliefRequestAsync(CreateReliefRequestDto request, CancellationToken cancellationToken = default)
        {
            var createdAt = DateTime.UtcNow;
            var currentUserId = _currentUserService.UserId;
            Domain.Entities.ApplicationUser? currentUser = null;

            if (currentUserId.HasValue)
            {
                currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId.Value);
            }

            var reliefRequest = new Domain.Entities.ReliefRequest
            {
                RequestId = Guid.NewGuid(),
                RequestType = RequestType.Relief,
                Description = request.Description ?? string.Empty,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Accuracy = request.Accuracy,
                Address = request.Address ?? string.Empty,
                LocationId = request.LocationId,
                ReporterUserId = currentUserId,
                ReporterFullName = currentUser?.UserName ?? request.ReporterFullName ?? "Anonymous",
                ReporterPhone = currentUser?.PhoneNumber ?? request.ReporterPhone,
                CreatedAt = createdAt,
                Status = ReliefRequestStatus.Pending
            };

            foreach (var need in request.NeedItems)
            {
                reliefRequest.ReliefNeedItems.Add(new ReliefNeedItem
                {
                    ReliefNeedItemId = Guid.NewGuid(),
                    ReliefRequestId = reliefRequest.RequestId,
                    NeedType = need.NeedType,
                    UrgencyLevel = need.UrgencyLevel,
                    PeopleCount = need.PeopleCount,
                    Note = need.Note
                });
            }

            if (request.Attachments != null)
            {
                foreach (var attachment in request.Attachments)
                {
                    reliefRequest.Attachments.Add(new Attachment
                    {
                        AttachmentId = Guid.NewGuid(),
                        RequestId = reliefRequest.RequestId,
                        FileUrl = attachment.FileUrl,
                        ContentType = attachment.ContentType,
                        UploadedAt = createdAt
                    });
                }
            }

            reliefRequest.Verifications.Add(new RequestVerification
            {
                RequestVerificationId = Guid.NewGuid(),
                RequestId = reliefRequest.RequestId,
                Status = RequestVerificationStatus.Pending,
                Method = VerificationMethod.None
            });

            var station = await SelectStationForReliefRequestAsync(reliefRequest.Latitude, reliefRequest.Longitude, cancellationToken);
            reliefRequest.AssignedReliefStationId = station.ReliefStationId;
            reliefRequest.CampaignId = await ResolveCampaignIdForReliefRequestAsync(station.ReliefStationId, cancellationToken);

            await _unitOfWork.ReliefRequests.AddAsync(reliefRequest);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await GetReliefRequestByIdAsync(reliefRequest.RequestId, cancellationToken);
        }

        public async Task<ReliefRequestResponseDto> GetReliefRequestByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
        {
            var request = await _unitOfWork.ReliefRequests.GetByIdAsync(requestId, cancellationToken);
            if (request == null)
            {
                throw new InvalidOperationException($"Relief request {requestId} not found");
            }

            return MapToResponseDto(request);
        }

        public async Task<PaginatedReliefRequestResponseDto> SearchReliefRequestsAsync(SearchReliefRequestDto request, CancellationToken cancellationToken = default)
        {
            ReliefRequestStatus? status = null;
            if (request.StatusFilter.HasValue)
            {
                status = (ReliefRequestStatus)request.StatusFilter.Value;
            }

            var (items, totalCount) = await _unitOfWork.ReliefRequests.SearchAsync(
                request.Search,
                request.PageNumber,
                request.PageSize,
                status,
                request.AssignedStationId,
                request.CampaignId,
                cancellationToken);

            return new PaginatedReliefRequestResponseDto
            {
                Data = items.Select(MapToResponseDto).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<ReliefRequestResponseDto> VerifyReliefRequestAsync(Guid requestId, VerifyReliefRequestDto dto, CancellationToken cancellationToken = default)
        {
            var request = await _unitOfWork.ReliefRequests.GetByIdAsync(requestId, cancellationToken);
            if (request == null)
            {
                throw new InvalidOperationException($"Relief request {requestId} not found");
            }

            var pendingVerification = request.Verifications
                .Where(v => v.Status == RequestVerificationStatus.Pending)
                .OrderByDescending(v => v.RequestVerificationId)
                .FirstOrDefault();

            if (pendingVerification == null)
            {
                throw new InvalidOperationException("No pending verification found for this relief request.");
            }

            pendingVerification.Status = dto.Status;
            pendingVerification.Method = dto.Method;
            pendingVerification.Note = dto.Note;
            pendingVerification.Reason = dto.Reason;
            pendingVerification.VerifiedBy = _currentUserService.UserId;
            pendingVerification.VerifiedAt = DateTime.UtcNow;

            if (dto.Status == RequestVerificationStatus.Approved)
            {
                request.Status = ReliefRequestStatus.Verified;
            }
            else if (dto.Status == RequestVerificationStatus.Rejected)
            {
                request.Status = ReliefRequestStatus.Rejected;
            }

            request.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponseDto(request);
        }

        public async Task<ReliefRequestResponseDto> ApproveReliefRequestAsync(Guid requestId, ApproveReliefRequestDto dto, CancellationToken cancellationToken = default)
        {
            var request = await _unitOfWork.ReliefRequests.GetByIdAsync(requestId, cancellationToken);
            if (request == null)
            {
                throw new InvalidOperationException($"Relief request {requestId} not found");
            }

            if (request.Status == ReliefRequestStatus.Rejected)
            {
                throw new InvalidOperationException("Rejected relief request cannot be approved.");
            }

            request.Status = ReliefRequestStatus.Approved;
            request.ApprovedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.Note))
            {
                request.Verifications.Add(new RequestVerification
                {
                    RequestVerificationId = Guid.NewGuid(),
                    RequestId = request.RequestId,
                    Status = RequestVerificationStatus.Approved,
                    Method = VerificationMethod.ManualReview,
                    Note = dto.Note,
                    VerifiedBy = _currentUserService.UserId,
                    VerifiedAt = DateTime.UtcNow
                });
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapToResponseDto(request);
        }

        public async Task<ReliefRequestResponseDto> RejectReliefRequestAsync(Guid requestId, RejectReliefRequestDto dto, CancellationToken cancellationToken = default)
        {
            var request = await _unitOfWork.ReliefRequests.GetByIdAsync(requestId, cancellationToken);
            if (request == null)
            {
                throw new InvalidOperationException($"Relief request {requestId} not found");
            }

            request.Status = ReliefRequestStatus.Rejected;
            request.UpdatedAt = DateTime.UtcNow;
            request.Verifications.Add(new RequestVerification
            {
                RequestVerificationId = Guid.NewGuid(),
                RequestId = request.RequestId,
                Status = RequestVerificationStatus.Rejected,
                Method = VerificationMethod.ManualReview,
                Reason = dto.Reason,
                VerifiedBy = _currentUserService.UserId,
                VerifiedAt = DateTime.UtcNow
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapToResponseDto(request);
        }

        public async Task<ReliefRequestResponseDto> AssignStationAsync(Guid requestId, AssignReliefRequestStationDto dto, CancellationToken cancellationToken = default)
        {
            var request = await _unitOfWork.ReliefRequests.GetByIdAsync(requestId, cancellationToken);
            if (request == null)
            {
                throw new InvalidOperationException($"Relief request {requestId} not found");
            }

            var station = await _unitOfWork.ReliefStations.GetByIdAsync(dto.ReliefStationId);
            if (station == null || station.ReliefStationStatus != ReliefStationStatus.Active)
            {
                throw new InvalidOperationException("Assigned relief station is invalid or inactive.");
            }

            request.AssignedReliefStationId = station.ReliefStationId;
            request.UpdatedAt = DateTime.UtcNow;

            if (!await IsCampaignValidForStationAsync(request.CampaignId, station.ReliefStationId, cancellationToken))
            {
                request.CampaignId = await ResolveCampaignIdForReliefRequestAsync(station.ReliefStationId, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await GetReliefRequestByIdAsync(requestId, cancellationToken);
        }

        public async Task<ReliefRequestResponseDto> AssignCampaignAsync(Guid requestId, AssignReliefRequestCampaignDto dto, CancellationToken cancellationToken = default)
        {
            var request = await _unitOfWork.ReliefRequests.GetByIdAsync(requestId, cancellationToken);
            if (request == null)
            {
                throw new InvalidOperationException($"Relief request {requestId} not found");
            }

            var campaign = await _unitOfWork.Campaigns.GetWithStationsAsync(dto.CampaignId, cancellationToken);
            if (campaign == null)
            {
                throw new InvalidOperationException("Campaign not found.");
            }

            if (campaign.Type != CampaignType.Relief)
            {
                throw new InvalidOperationException("Only relief campaigns can be assigned to relief requests.");
            }

            if (request.AssignedReliefStationId.HasValue)
            {
                var hasActiveStation = campaign.CampaignStations.Any(cs =>
                    cs.ReliefStationId == request.AssignedReliefStationId.Value && cs.IsActive);

                if (!hasActiveStation)
                {
                    throw new InvalidOperationException("Campaign does not have the assigned relief station attached as active.");
                }
            }

            request.CampaignId = campaign.CampaignId;
            request.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await GetReliefRequestByIdAsync(requestId, cancellationToken);
        }

        private async Task<ReliefStation> SelectStationForReliefRequestAsync(double requestLatitude, double requestLongitude, CancellationToken cancellationToken)
        {
            var stations = await _unitOfWork.ReliefStations.GetAllAsync();
            var activeStations = stations
                .Where(s => s.ReliefStationStatus == ReliefStationStatus.Active)
                .ToList();

            if (activeStations.Count == 0)
            {
                throw new InvalidOperationException("No active relief station available for assignment.");
            }

            var scoredStations = activeStations
                .Select(s => new StationDistanceScore
                {
                    Station = s,
                    DistanceKm = CalculateDistance(requestLatitude, requestLongitude, s.Latitude, s.Longitude)
                })
                .ToList();

            var withinCoverage = scoredStations
                .Where(s => s.DistanceKm <= s.Station.CoverageRadiusKm)
                .OrderBy(s => s.DistanceKm)
                .FirstOrDefault();

            return withinCoverage?.Station
                ?? scoredStations.OrderBy(s => s.DistanceKm).First().Station;
        }

        private async Task<Guid?> ResolveCampaignIdForReliefRequestAsync(Guid reliefStationId, CancellationToken cancellationToken)
        {
            var campaigns = await SearchActiveReliefCampaignsByStationAsync(reliefStationId, cancellationToken);
            return campaigns
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => (Guid?)c.CampaignId)
                .FirstOrDefault();
        }

        private async Task<bool> IsCampaignValidForStationAsync(Guid? campaignId, Guid reliefStationId, CancellationToken cancellationToken)
        {
            if (!campaignId.HasValue)
            {
                return false;
            }

            var campaign = await _unitOfWork.Campaigns.GetWithStationsAsync(campaignId.Value, cancellationToken);
            return campaign != null
                && campaign.Type == CampaignType.Relief
                && campaign.Status == CampaignStatus.Active
                && campaign.CampaignStations.Any(cs => cs.ReliefStationId == reliefStationId && cs.IsActive);
        }

        private async Task<List<Campaign>> SearchActiveReliefCampaignsByStationAsync(Guid reliefStationId, CancellationToken cancellationToken)
        {
            return await _unitOfWork.Campaigns.GetActiveReliefCampaignsByStationAsync(reliefStationId, cancellationToken);
        }

        private ReliefRequestResponseDto MapToResponseDto(Domain.Entities.ReliefRequest request)
        {
            return new ReliefRequestResponseDto
            {
                RequestId = request.RequestId,
                Description = request.Description,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Accuracy = request.Accuracy,
                Address = request.Address,
                LocationId = request.LocationId,
                ReporterFullName = request.ReporterFullName,
                ReporterPhone = request.ReporterPhone,
                Status = request.Status.ToString(),
                CampaignId = request.CampaignId,
                CampaignName = request.Campaign?.Name,
                AssignedReliefStationId = request.AssignedReliefStationId,
                AssignedReliefStationName = request.AssignedReliefStation?.Name,
                CreatedAt = request.CreatedAt,
                UpdatedAt = request.UpdatedAt,
                ApprovedAt = request.ApprovedAt,
                CompletedAt = request.CompletedAt,
                NeedItems = request.ReliefNeedItems.Select(n => new ReliefNeedItemResponseDto
                {
                    ReliefNeedItemId = n.ReliefNeedItemId,
                    NeedType = n.NeedType,
                    UrgencyLevel = n.UrgencyLevel,
                    PeopleCount = n.PeopleCount,
                    Note = n.Note
                }).ToList(),
                Attachments = request.Attachments.Select(a => new ReliefAttachmentResponseDto
                {
                    AttachmentId = a.AttachmentId,
                    FileUrl = a.FileUrl,
                    ContentType = a.ContentType,
                    UploadedAt = a.UploadedAt
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

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371;
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180d;

        private sealed class StationDistanceScore
        {
            public ReliefStation Station { get; set; } = default!;
            public double DistanceKm { get; set; }
        }
    }
}
