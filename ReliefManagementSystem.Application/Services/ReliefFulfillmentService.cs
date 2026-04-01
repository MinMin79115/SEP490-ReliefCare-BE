using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.ReliefFulfillment.DTOs.Request;
using ReliefManagementSystem.Application.Features.ReliefFulfillment.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    public class ReliefFulfillmentService : IReliefFulfillmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public ReliefFulfillmentService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ReliefFulfillmentResponseDto> CreateAsync(Guid distributionSessionId, CreateReliefFulfillmentRequest request, CancellationToken cancellationToken = default)
        {
            var session = await _unitOfWork.DistributionSessions.GetByIdAsync(distributionSessionId, cancellationToken);
            if (session == null)
                throw new KeyNotFoundException($"Distribution session '{distributionSessionId}' was not found.");

            if (session.Status != DistributionSessionStatus.InProgress)
                throw new InvalidOperationException("Relief fulfillment can only be recorded when distribution session is InProgress.");

            var reliefRequest = await _unitOfWork.ReliefRequests.GetByIdAsync(request.ReliefRequestId, cancellationToken);
            if (reliefRequest == null)
                throw new KeyNotFoundException($"Relief request '{request.ReliefRequestId}' was not found.");

            var attached = session.Requests.Any(x => x.ReliefRequestId == request.ReliefRequestId);
            if (!attached)
                throw new InvalidOperationException("Relief request is not attached to this distribution session.");

            if (reliefRequest.Status != ReliefRequestStatus.Allocated && reliefRequest.Status != ReliefRequestStatus.Delivered)
                throw new InvalidOperationException("Only Allocated or Delivered relief requests can record fulfillment.");

            var previousFulfillments = await _unitOfWork.ReliefFulfillments.GetByRequestAsync(reliefRequest.RequestId, cancellationToken);
            var waveNumber = previousFulfillments.Count + 1;
            var deliveredAt = request.DeliveredAt ?? DateTime.UtcNow;

            var fulfillment = new Domain.Entities.ReliefFulfillment
            {
                ReliefFulfillmentId = Guid.NewGuid(),
                ReliefRequestId = reliefRequest.RequestId,
                DistributionSessionId = session.DistributionSessionId,
                WaveNumber = waveNumber,
                Mode = session.Mode,
                Status = ReliefFulfillmentStatus.Delivered,
                DeliveredAt = deliveredAt,
                RecipientName = request.RecipientName,
                RecipientPhone = request.RecipientPhone,
                DeliveryNote = request.DeliveryNote,
                ProofImageUrl = request.ProofImageUrl,
                CreatedBy = _currentUserService.UserId,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var item in request.Items)
            {
                if (!await _unitOfWork.SupplyItems.ExistsAsync(item.SupplyItemId))
                    throw new KeyNotFoundException($"Supply item '{item.SupplyItemId}' was not found.");

                fulfillment.Items.Add(new ReliefFulfillmentItem
                {
                    ReliefFulfillmentItemId = Guid.NewGuid(),
                    ReliefFulfillmentId = fulfillment.ReliefFulfillmentId,
                    SupplyItemId = item.SupplyItemId,
                    NeedCategory = item.NeedCategory,
                    PlannedQuantity = item.PlannedQuantity,
                    ActualDeliveredQuantity = item.ActualDeliveredQuantity,
                    Note = item.Note
                });

                var sessionItems = session.Items.Where(x => x.SupplyItemId == item.SupplyItemId).ToList();
                if (sessionItems.Count > 0)
                {
                    var remaining = item.ActualDeliveredQuantity;
                    foreach (var sessionItem in sessionItems)
                    {
                        sessionItem.DeliveredQuantity += remaining;
                        break;
                    }
                }
            }

            await _unitOfWork.ReliefFulfillments.AddAsync(fulfillment);

            if (reliefRequest.Status == ReliefRequestStatus.Allocated)
            {
                reliefRequest.Status = ReliefRequestStatus.Delivered;
                reliefRequest.UpdatedAt = DateTime.UtcNow;
            }

            session.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var saved = await _unitOfWork.ReliefFulfillments.GetByIdAsync(fulfillment.ReliefFulfillmentId, cancellationToken);
            return MapToResponse(saved!);
        }

        public async Task<IReadOnlyList<ReliefFulfillmentResponseDto>> GetByRequestAsync(Guid reliefRequestId, CancellationToken cancellationToken = default)
        {
            var items = await _unitOfWork.ReliefFulfillments.GetByRequestAsync(reliefRequestId, cancellationToken);
            return items.Select(MapToResponse).ToList();
        }

        public async Task<IReadOnlyList<ReliefFulfillmentResponseDto>> GetBySessionAsync(Guid distributionSessionId, CancellationToken cancellationToken = default)
        {
            var items = await _unitOfWork.ReliefFulfillments.GetBySessionAsync(distributionSessionId, cancellationToken);
            return items.Select(MapToResponse).ToList();
        }

        public async Task<ReliefFulfillmentResponseDto> AddProofAsync(Guid reliefFulfillmentId, UpdateReliefFulfillmentProofRequest request, CancellationToken cancellationToken = default)
        {
            var fulfillment = await _unitOfWork.ReliefFulfillments.GetByIdAsync(reliefFulfillmentId, cancellationToken);
            if (fulfillment == null)
                throw new KeyNotFoundException($"Relief fulfillment '{reliefFulfillmentId}' was not found.");

            fulfillment.ProofImageUrl = request.ProofImageUrl ?? fulfillment.ProofImageUrl;
            fulfillment.DeliveryNote = request.DeliveryNote ?? fulfillment.DeliveryNote;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapToResponse(fulfillment);
        }

        public async Task<ReliefFulfillmentResponseDto> MarkFailedAsync(Guid reliefFulfillmentId, MarkReliefFulfillmentFailedRequest request, CancellationToken cancellationToken = default)
        {
            var fulfillment = await _unitOfWork.ReliefFulfillments.GetByIdAsync(reliefFulfillmentId, cancellationToken);
            if (fulfillment == null)
                throw new KeyNotFoundException($"Relief fulfillment '{reliefFulfillmentId}' was not found.");

            if (fulfillment.Status == ReliefFulfillmentStatus.Delivered)
                throw new InvalidOperationException("Delivered fulfillment cannot be marked failed.");

            fulfillment.Status = ReliefFulfillmentStatus.Failed;
            fulfillment.DeliveryNote = request.Note ?? fulfillment.DeliveryNote;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapToResponse(fulfillment);
        }

        private static ReliefFulfillmentResponseDto MapToResponse(Domain.Entities.ReliefFulfillment fulfillment)
        {
            return new ReliefFulfillmentResponseDto
            {
                ReliefFulfillmentId = fulfillment.ReliefFulfillmentId,
                ReliefRequestId = fulfillment.ReliefRequestId,
                DistributionSessionId = fulfillment.DistributionSessionId,
                WaveNumber = fulfillment.WaveNumber,
                Mode = fulfillment.Mode,
                Status = fulfillment.Status,
                DeliveredAt = fulfillment.DeliveredAt,
                RecipientName = fulfillment.RecipientName,
                RecipientPhone = fulfillment.RecipientPhone,
                DeliveryNote = fulfillment.DeliveryNote,
                ProofImageUrl = fulfillment.ProofImageUrl,
                CreatedBy = fulfillment.CreatedBy,
                CreatedAt = fulfillment.CreatedAt,
                Items = fulfillment.Items.Select(i => new ReliefFulfillmentItemResponseDto
                {
                    ReliefFulfillmentItemId = i.ReliefFulfillmentItemId,
                    SupplyItemId = i.SupplyItemId,
                    SupplyItemName = i.SupplyItem?.Name ?? string.Empty,
                    NeedCategory = i.NeedCategory,
                    PlannedQuantity = i.PlannedQuantity,
                    ActualDeliveredQuantity = i.ActualDeliveredQuantity,
                    Note = i.Note
                }).ToList()
            };
        }
    }
}
