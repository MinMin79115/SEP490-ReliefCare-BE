using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.InventoryTransaction.DTOs.Request;
using ReliefManagementSystem.Application.Features.SupplyTransfer.DTOs.Request;
using ReliefManagementSystem.Application.Features.SupplyTransfer.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    public class SupplyTransferService : ISupplyTransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryTransactionService _inventoryTransactionService;
        private readonly ICurrentUserService _currentUser;

        public SupplyTransferService(
            IUnitOfWork unitOfWork,
            IInventoryTransactionService inventoryTransactionService,
            ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _inventoryTransactionService = inventoryTransactionService;
            _currentUser = currentUser;
        }

        public async Task<SupplyTransferResponse> CreateAsync(CreateSupplyTransferRequest request, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");

            if (request.SourceStationId == request.DestinationStationId)
                throw new InvalidOperationException("Source station và destination station không được trùng nhau.");

            var sourceStation = await _unitOfWork.ReliefStations.GetByIdAsync(request.SourceStationId)
                ?? throw new KeyNotFoundException($"Source station '{request.SourceStationId}' was not found.");
            var destinationStation = await _unitOfWork.ReliefStations.GetByIdAsync(request.DestinationStationId)
                ?? throw new KeyNotFoundException($"Destination station '{request.DestinationStationId}' was not found.");

            if (sourceStation.ReliefStationStatus != ReliefStationStatus.Active || destinationStation.ReliefStationStatus != ReliefStationStatus.Active)
                throw new InvalidOperationException("Chỉ được tạo yêu cầu giữa các trạm đang Active.");

            var destinationHead = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(request.DestinationStationId, cancellationToken)
                ?? throw new InvalidOperationException("Trạm đích chưa có station head active để tạo yêu cầu.");

            if (destinationHead.UserId != currentUserId)
                throw new InvalidOperationException("Chỉ station head của trạm đích mới được tạo yêu cầu điều chuyển.");

            if (request.Items is null || request.Items.Count == 0)
                throw new InvalidOperationException("At least one item is required.");

            var duplicates = request.Items.GroupBy(x => x.SupplyItemId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicates.Count != 0)
                throw new InvalidOperationException($"Duplicate supply items in request: {string.Join(", ", duplicates)}");

            var sourceInventory = await _unitOfWork.Inventories.GetActiveByReliefStationAsync(request.SourceStationId, cancellationToken)
                ?? throw new InvalidOperationException("Trạm nguồn chưa có inventory active.");
            var sourceStocks = await _unitOfWork.InventoryStocks.GetByInventoryIdAsync(sourceInventory.InventoryId, cancellationToken);

            foreach (var item in request.Items)
            {
                var stock = sourceStocks.FirstOrDefault(s => s.SupplyItemId == item.SupplyItemId);
                if (stock is null)
                    throw new InvalidOperationException($"Supply item '{item.SupplyItemId}' is not registered in source inventory.");
            }

            var transfer = new SupplyTransfer
            {
                SupplyTransferId = Guid.NewGuid(),
                TransferCode = await GenerateTransferCodeAsync(cancellationToken),
                SourceStationId = request.SourceStationId,
                DestinationStationId = request.DestinationStationId,
                Status = SupplyTransferStatus.Pending,
                RequestedAt = DateTime.UtcNow,
                RequestedBy = currentUserId,
                Notes = BuildNotes(request.Reason, request.Notes),
                EvidenceUrls = NormalizeEvidenceUrls(request.EvidenceUrls),
                Items = request.Items.Select(i => new SupplyTransferItem
                {
                    SupplyTransferItemId = Guid.NewGuid(),
                    SupplyItemId = i.SupplyItemId,
                    RequestedQuantity = i.Quantity,
                    Notes = i.Notes
                }).ToList()
            };

            await _unitOfWork.SupplyTransfers.AddAsync(transfer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var saved = await _unitOfWork.SupplyTransfers.GetByIdWithDetailsAsync(transfer.SupplyTransferId, cancellationToken);
            return MapToResponse(saved!);
        }

        public async Task<SupplyTransferResponse> GetByIdAsync(Guid transferId, CancellationToken cancellationToken = default)
        {
            var transfer = await _unitOfWork.SupplyTransfers.GetByIdWithDetailsAsync(transferId, cancellationToken)
                ?? throw new KeyNotFoundException($"Supply transfer '{transferId}' was not found.");
            return MapToResponse(transfer);
        }

        public async Task<IReadOnlyList<SupplyTransferSummaryResponse>> GetByStatusAsync(SupplyTransferStatus status, CancellationToken cancellationToken = default)
            => (await _unitOfWork.SupplyTransfers.GetByStatusAsync(status, cancellationToken)).Select(MapToSummary).ToList();

        public async Task<IReadOnlyList<SupplyTransferSummaryResponse>> GetBySourceStationAsync(Guid stationId, CancellationToken cancellationToken = default)
            => (await _unitOfWork.SupplyTransfers.GetBySourceStationAsync(stationId, cancellationToken)).Select(MapToSummary).ToList();

        public async Task<IReadOnlyList<SupplyTransferSummaryResponse>> GetByDestinationStationAsync(Guid stationId, CancellationToken cancellationToken = default)
            => (await _unitOfWork.SupplyTransfers.GetByDestinationStationAsync(stationId, cancellationToken)).Select(MapToSummary).ToList();

        public async Task<SupplyTransferResponse> ApproveAsync(Guid transferId, ApproveSupplyTransferRequest request, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var transfer = await LoadTransferForUpdateAsync(transferId, cancellationToken);

            if (transfer.Status != SupplyTransferStatus.Pending)
                throw new InvalidOperationException("Chỉ yêu cầu Pending mới được duyệt.");

            var sourceHead = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(transfer.SourceStationId, cancellationToken)
                ?? throw new InvalidOperationException("Trạm nguồn chưa có station head active để duyệt yêu cầu.");

            if (sourceHead.UserId != currentUserId)
                throw new InvalidOperationException("Chỉ station head của trạm nguồn mới được duyệt yêu cầu điều chuyển.");

            transfer.Status = SupplyTransferStatus.Approved;
            transfer.ApprovedAt = DateTime.UtcNow;
            transfer.ApprovedBy = currentUserId;
            transfer.Notes = AppendNotes(transfer.Notes, request.Notes);
            transfer.EvidenceUrls = MergeEvidenceUrls(transfer.EvidenceUrls, request.EvidenceUrls);
            await _unitOfWork.SupplyTransfers.UpdateAsync(transfer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse((await _unitOfWork.SupplyTransfers.GetByIdWithDetailsAsync(transferId, cancellationToken))!);
        }

        public async Task<SupplyTransferResponse> ShipAsync(Guid transferId, ShipSupplyTransferRequest request, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var transfer = await LoadTransferForUpdateAsync(transferId, cancellationToken);

            if (transfer.Status != SupplyTransferStatus.Approved)
                throw new InvalidOperationException("Chỉ yêu cầu đã duyệt mới được chuyển sang Shipping.");

            var sourceHead = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(transfer.SourceStationId, cancellationToken)
                ?? throw new InvalidOperationException("Trạm nguồn chưa có station head active để xuất hàng.");

            if (sourceHead.UserId != currentUserId)
                throw new InvalidOperationException("Chỉ station head của trạm nguồn mới được xuất hàng.");

            var sourceInventory = await _unitOfWork.Inventories.GetActiveByReliefStationAsync(transfer.SourceStationId, cancellationToken)
                ?? throw new InvalidOperationException("Trạm nguồn chưa có inventory active.");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var existingOutboundTransaction = transfer.InventoryTransactions
                    .FirstOrDefault(t => t.Reason == TransactionReason.SupplyTransferOut);

                if (existingOutboundTransaction is null)
                {
                    await _inventoryTransactionService.CreateTransactionAsync(new CreateTransactionRequest
                    {
                        InventoryId = sourceInventory.InventoryId,
                        SupplyTransferId = transfer.SupplyTransferId,
                        Type = TransactionType.Export,
                        Reason = TransactionReason.SupplyTransferOut,
                        Notes = $"Supply transfer shipping: {transfer.TransferCode}",
                        Items = transfer.Items.Select(i => new TransactionItemRequest
                        {
                            SupplyItemId = i.SupplyItemId,
                            Quantity = i.RequestedQuantity,
                            Notes = i.Notes
                        }).ToList()
                    }, autoSave: false, cancellationToken);
                }

                var activeAssignments = transfer.SupplyTransferVehicles.Where(v => v.Status == SupplyTransferVehicleStatus.Assigned || v.Status == SupplyTransferVehicleStatus.Incident).ToList();
                if (activeAssignments.Count == 0) throw new InvalidOperationException("Không thể xuất hàng khi chưa có xe được phân công.");
                foreach (var assignment in activeAssignments.Where(v => v.Status == SupplyTransferVehicleStatus.Assigned))
                {
                    assignment.Status = SupplyTransferVehicleStatus.InTransit;
                    assignment.DepartedAt = DateTime.UtcNow;
                }
                transfer.Status = SupplyTransferStatus.Shipping;
                transfer.ShippedAt = DateTime.UtcNow;
                transfer.VehicleId = transfer.SupplyTransferVehicles.FirstOrDefault()?.VehicleId ?? request.VehicleId;
                transfer.DriverUserId = transfer.SupplyTransferVehicles.FirstOrDefault()?.DriverUserId;
                transfer.Notes = AppendNotes(transfer.Notes, request.Notes);
                transfer.EvidenceUrls = MergeEvidenceUrls(transfer.EvidenceUrls, request.EvidenceUrls);
                await _unitOfWork.SupplyTransfers.UpdateAsync(transfer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return MapToResponse((await _unitOfWork.SupplyTransfers.GetByIdWithDetailsAsync(transferId, cancellationToken))!);
        }

        public async Task<SupplyTransferResponse> ReceiveAsync(Guid transferId, ReceiveSupplyTransferRequest request, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var transfer = await LoadTransferForUpdateAsync(transferId, cancellationToken);

            if (transfer.Status != SupplyTransferStatus.Shipping)
                throw new InvalidOperationException("Chỉ yêu cầu đang Shipping mới được xác nhận nhận hàng.");

            var destinationHead = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(transfer.DestinationStationId, cancellationToken)
                ?? throw new InvalidOperationException("Trạm đích chưa có station head active để xác nhận nhận hàng.");

            if (destinationHead.UserId != currentUserId)
                throw new InvalidOperationException("Chỉ station head của trạm đích mới được xác nhận nhận hàng.");

            var destinationInventory = await _unitOfWork.Inventories.GetActiveByReliefStationAsync(transfer.DestinationStationId, cancellationToken)
                ?? throw new InvalidOperationException("Trạm đích chưa có inventory active.");

            if (request.Items is null || request.Items.Count == 0)
                throw new InvalidOperationException("At least one received item is required.");

            var actualBySupplyId = request.Items.ToDictionary(i => i.SupplyItemId);
            foreach (var item in transfer.Items)
            {
                if (!actualBySupplyId.TryGetValue(item.SupplyItemId, out var actual))
                    throw new InvalidOperationException($"Thiếu actual quantity cho supply item '{item.SupplyItemId}'.");
                if (actual.ActualQuantity > item.RequestedQuantity)
                    throw new InvalidOperationException("Actual quantity không được lớn hơn requested quantity.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var item in transfer.Items)
                {
                    await EnsureInventoryStockExistsAsync(destinationInventory.InventoryId, item.SupplyItemId, cancellationToken);
                }

                // Persist newly created destination stocks before creating import transaction,
                // because CreateTransactionAsync reloads inventory stocks from the database.
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var transaction = await _inventoryTransactionService.CreateTransactionAsync(new CreateTransactionRequest
                {
                    InventoryId = destinationInventory.InventoryId,
                    SupplyTransferId = transfer.SupplyTransferId,
                    Type = TransactionType.Import,
                    Reason = TransactionReason.SupplyTransferIn,
                    Notes = $"Supply transfer received: {transfer.TransferCode}",
                    Items = transfer.Items
                        .Select(i => actualBySupplyId[i.SupplyItemId])
                        .Where(i => i.ActualQuantity > 0)
                        .Select(i => new TransactionItemRequest
                        {
                            SupplyItemId = i.SupplyItemId,
                            Quantity = i.ActualQuantity,
                            Notes = i.Notes
                        }).ToList()
                }, autoSave: false, cancellationToken);

                foreach (var item in transfer.Items)
                {
                    var actual = actualBySupplyId[item.SupplyItemId];
                    item.ActualQuantity = actual.ActualQuantity;
                    item.Notes = string.IsNullOrWhiteSpace(actual.Notes) ? item.Notes : actual.Notes;
                }

                transfer.Status = SupplyTransferStatus.Received;
                transfer.ReceivedAt = DateTime.UtcNow;
                foreach (var assignment in transfer.SupplyTransferVehicles.Where(v => v.Status == SupplyTransferVehicleStatus.Assigned || v.Status == SupplyTransferVehicleStatus.InTransit || v.Status == SupplyTransferVehicleStatus.Arrived || v.Status == SupplyTransferVehicleStatus.Incident))
                {
                    assignment.Status = SupplyTransferVehicleStatus.Completed;
                    assignment.CompletedAt = DateTime.UtcNow;
                    assignment.Vehicle.Status = VehicleStatus.Free;
                }
                transfer.Notes = AppendNotes(transfer.Notes, request.Notes);
                transfer.EvidenceUrls = MergeEvidenceUrls(transfer.EvidenceUrls, request.EvidenceUrls);
                await _unitOfWork.SupplyTransfers.UpdateAsync(transfer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return MapToResponse((await _unitOfWork.SupplyTransfers.GetByIdWithDetailsAsync(transferId, cancellationToken))!);
        }

        public async Task<SupplyTransferResponse> CancelAsync(Guid transferId, CancelSupplyTransferRequest request, CancellationToken cancellationToken = default)
        {
            var transfer = await LoadTransferForUpdateAsync(transferId, cancellationToken);
            if (transfer.Status is SupplyTransferStatus.Shipping or SupplyTransferStatus.Received)
                throw new InvalidOperationException("Không thể hủy yêu cầu đã xuất hàng hoặc đã nhận hàng.");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                transfer.Status = SupplyTransferStatus.Cancelled;
                foreach (var assignment in transfer.SupplyTransferVehicles.Where(IsActiveAssignment))
                {
                    assignment.Status = SupplyTransferVehicleStatus.Cancelled;
                    assignment.CancelledAt = DateTime.UtcNow;
                    assignment.Vehicle.Status = VehicleStatus.Free;
                }
                transfer.Notes = AppendNotes(transfer.Notes, request.Notes);
                transfer.EvidenceUrls = MergeEvidenceUrls(transfer.EvidenceUrls, request.EvidenceUrls);
                await _unitOfWork.SupplyTransfers.UpdateAsync(transfer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch { await _unitOfWork.RollbackTransactionAsync(cancellationToken); throw; }
            return MapToResponse((await _unitOfWork.SupplyTransfers.GetByIdWithDetailsAsync(transferId, cancellationToken))!);
        }

        public async Task<SupplyTransferResponse> AssignVehiclesAsync(Guid transferId, AssignSupplyTransferVehiclesRequest request, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var transfer = await LoadTransferForUpdateAsync(transferId, cancellationToken);
            if (transfer.Status != SupplyTransferStatus.Approved) throw new InvalidOperationException("Chỉ yêu cầu Approved mới được gán xe.");
            var sourceHead = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(transfer.SourceStationId, cancellationToken)
                ?? throw new InvalidOperationException("Trạm nguồn chưa có station head active để gán xe.");
            if (sourceHead.UserId != currentUserId) throw new InvalidOperationException("Chỉ station head của trạm nguồn mới được gán xe.");
            if (request.Vehicles is null || request.Vehicles.Count == 0) throw new InvalidOperationException("Cần ít nhất một xe để gán.");
            if (request.Vehicles.Select(v => v.VehicleId).Distinct().Count() != request.Vehicles.Count) throw new InvalidOperationException("Không được gán trùng xe.");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                foreach (var item in request.Vehicles)
                {
                    if (transfer.SupplyTransferVehicles.Any(v => v.VehicleId == item.VehicleId)) throw new InvalidOperationException("Xe đã được gán cho phiếu này.");
                    var vehicle = await _unitOfWork.Vehicles.GetByIdWithDetailsAsync(item.VehicleId) ?? throw new KeyNotFoundException($"Vehicle '{item.VehicleId}' was not found.");
                    if (vehicle.ReliefStationId != transfer.SourceStationId) throw new InvalidOperationException("Xe không thuộc trạm nguồn.");
                    if (vehicle.Status != VehicleStatus.Free) throw new InvalidOperationException("Xe không ở trạng thái Free.");
                    var activeRescueOperation = await _unitOfWork.RescueOperations.GetActiveByVehicleIdAsync(vehicle.VehicleId, cancellationToken);
                    if (activeRescueOperation is not null) throw new InvalidOperationException("Xe đang được dùng trong luồng cứu hộ.");
                    vehicle.Status = VehicleStatus.Busy;
                    transfer.SupplyTransferVehicles.Add(new SupplyTransferVehicle { SupplyTransferVehicleId = Guid.NewGuid(), SupplyTransferId = transfer.SupplyTransferId, VehicleId = item.VehicleId, DriverUserId = item.DriverUserId, Status = SupplyTransferVehicleStatus.Assigned, AssignedAt = DateTime.UtcNow, Note = item.Note });
                }
                await _unitOfWork.SupplyTransfers.UpdateAsync(transfer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch { await _unitOfWork.RollbackTransactionAsync(cancellationToken); throw; }
            return MapToResponse((await _unitOfWork.SupplyTransfers.GetByIdWithDetailsAsync(transferId, cancellationToken))!);
        }

        public async Task<SupplyTransferResponse> RemoveVehicleAsync(Guid transferId, Guid supplyTransferVehicleId, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var transfer = await LoadTransferForUpdateAsync(transferId, cancellationToken);
            if (transfer.Status != SupplyTransferStatus.Approved) throw new InvalidOperationException("Chỉ được bỏ xe trước khi xuất hàng.");
            var sourceHead = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(transfer.SourceStationId, cancellationToken)
                ?? throw new InvalidOperationException("Trạm nguồn chưa có station head active để bỏ xe.");
            if (sourceHead.UserId != currentUserId) throw new InvalidOperationException("Chỉ station head của trạm nguồn mới được bỏ xe.");

            var assignment = transfer.SupplyTransferVehicles.FirstOrDefault(x => x.SupplyTransferVehicleId == supplyTransferVehicleId)
                ?? throw new KeyNotFoundException("Transfer vehicle assignment not found.");
            if (assignment.Status != SupplyTransferVehicleStatus.Assigned) throw new InvalidOperationException("Chỉ được bỏ xe đang ở trạng thái Assigned.");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                assignment.Status = SupplyTransferVehicleStatus.Cancelled;
                assignment.CancelledAt = DateTime.UtcNow;
                assignment.Vehicle.Status = VehicleStatus.Free;
                await _unitOfWork.SupplyTransfers.UpdateAsync(transfer);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch { await _unitOfWork.RollbackTransactionAsync(cancellationToken); throw; }

            return MapToResponse((await _unitOfWork.SupplyTransfers.GetByIdWithDetailsAsync(transferId, cancellationToken))!);
        }

        public async Task<SupplyTransferResponse> UpdateVehicleStatusAsync(Guid transferId, Guid supplyTransferVehicleId, UpdateSupplyTransferVehicleStatusRequest request, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var transfer = await LoadTransferForUpdateAsync(transferId, cancellationToken);
            var sourceHead = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(transfer.SourceStationId, cancellationToken);
            var destinationHead = await _unitOfWork.ModeratorProfiles.GetStationHeadAsync(transfer.DestinationStationId, cancellationToken);
            var canUpdate = request.Status switch
            {
                SupplyTransferVehicleStatus.InTransit or SupplyTransferVehicleStatus.Incident => sourceHead?.UserId == currentUserId,
                SupplyTransferVehicleStatus.Arrived or SupplyTransferVehicleStatus.Completed => destinationHead?.UserId == currentUserId,
                _ => false
            };
            if (!canUpdate) throw new InvalidOperationException("Bạn không có quyền cập nhật trạng thái xe trong phiếu này.");
            var assignment = transfer.SupplyTransferVehicles.FirstOrDefault(x => x.SupplyTransferVehicleId == supplyTransferVehicleId) ?? throw new KeyNotFoundException("Transfer vehicle assignment not found.");
            assignment.Note = AppendNotes(assignment.Note, request.Note);
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
            if (request.Status == SupplyTransferVehicleStatus.InTransit) { assignment.Status = SupplyTransferVehicleStatus.InTransit; assignment.DepartedAt ??= DateTime.UtcNow; if (transfer.Status == SupplyTransferStatus.Approved) transfer.Status = SupplyTransferStatus.Shipping; }
            else if (request.Status == SupplyTransferVehicleStatus.Arrived) { assignment.Status = SupplyTransferVehicleStatus.Arrived; assignment.ArrivedAt = DateTime.UtcNow; }
            else if (request.Status == SupplyTransferVehicleStatus.Completed) { assignment.Status = SupplyTransferVehicleStatus.Completed; assignment.CompletedAt = DateTime.UtcNow; assignment.Vehicle.Status = VehicleStatus.Free; }
            else if (request.Status == SupplyTransferVehicleStatus.Incident) { assignment.Status = SupplyTransferVehicleStatus.Incident; }
            else throw new InvalidOperationException("Unsupported status update.");
            await _unitOfWork.SupplyTransfers.UpdateAsync(transfer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch { await _unitOfWork.RollbackTransactionAsync(cancellationToken); throw; }
            return MapToResponse((await _unitOfWork.SupplyTransfers.GetByIdWithDetailsAsync(transferId, cancellationToken))!);
        }

        public async Task<SupplyTransferResponse> ReplaceEvidenceUrlsAsync(Guid transferId, ReplaceSupplyTransferEvidenceUrlsRequest request, CancellationToken cancellationToken = default)
        {
            var transfer = await LoadTransferForUpdateAsync(transferId, cancellationToken);
            transfer.EvidenceUrls = NormalizeEvidenceUrls(request.EvidenceUrls);
            await _unitOfWork.SupplyTransfers.UpdateAsync(transfer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse((await _unitOfWork.SupplyTransfers.GetByIdWithDetailsAsync(transferId, cancellationToken))!);
        }

        public async Task<SupplyTransferResponse> AppendEvidenceUrlsAsync(Guid transferId, AppendSupplyTransferEvidenceUrlsRequest request, CancellationToken cancellationToken = default)
        {
            var transfer = await LoadTransferForUpdateAsync(transferId, cancellationToken);
            transfer.EvidenceUrls = MergeEvidenceUrls(transfer.EvidenceUrls, request.EvidenceUrls);
            await _unitOfWork.SupplyTransfers.UpdateAsync(transfer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse((await _unitOfWork.SupplyTransfers.GetByIdWithDetailsAsync(transferId, cancellationToken))!);
        }

        public async Task<SupplyTransferResponse> AddDocumentAsync(Guid transferId, CreateSupplyTransferDocumentRequest request, CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var transfer = await LoadTransferForUpdateAsync(transferId, cancellationToken);

            var normalizedUrl = NormalizeRequiredUrl(request.FileUrl);
            var fileName = NormalizeOptional(request.FileName, 255);
            var contentType = NormalizeOptional(request.ContentType, 100);
            var notes = NormalizeOptional(request.Notes, 1000);

            var currentDocumentsOfType = transfer.Documents
                .Where(d => d.DocumentType == request.DocumentType)
                .ToList();

            foreach (var currentDocument in currentDocumentsOfType.Where(d => d.IsCurrent))
            {
                currentDocument.IsCurrent = false;
            }

            var nextVersion = currentDocumentsOfType.Count == 0
                ? 1
                : currentDocumentsOfType.Max(d => d.Version) + 1;

            transfer.Documents.Add(new SupplyTransferDocument
            {
                SupplyTransferDocumentId = Guid.NewGuid(),
                SupplyTransferId = transfer.SupplyTransferId,
                DocumentType = request.DocumentType,
                Version = nextVersion,
                FileUrl = normalizedUrl,
                FileName = fileName,
                ContentType = contentType,
                FileSizeBytes = request.FileSizeBytes,
                IsCurrent = true,
                CreatedBy = currentUserId,
                CreatedAt = DateTime.UtcNow,
                Notes = notes
            });

            await _unitOfWork.SupplyTransfers.UpdateAsync(transfer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse((await _unitOfWork.SupplyTransfers.GetByIdWithDetailsAsync(transferId, cancellationToken))!);
        }

        private async Task<SupplyTransfer> LoadTransferForUpdateAsync(Guid transferId, CancellationToken cancellationToken)
            => await _unitOfWork.SupplyTransfers.GetByIdWithDetailsAsync(transferId, cancellationToken)
               ?? throw new KeyNotFoundException($"Supply transfer '{transferId}' was not found.");

        private async Task EnsureInventoryStockExistsAsync(Guid inventoryId, Guid supplyItemId, CancellationToken cancellationToken)
        {
            var stock = await _unitOfWork.InventoryStocks.GetByInventoryAndSupplyItemAsync(inventoryId, supplyItemId, cancellationToken);
            if (stock != null)
            {
                return;
            }

            await _unitOfWork.InventoryStocks.AddAsync(new InventoryStock
            {
                InventoryStockId = Guid.NewGuid(),
                InventoryId = inventoryId,
                SupplyItemId = supplyItemId,
                CurrentQuantity = 0,
                MinimumStockLevel = 0,
                MaximumStockLevel = int.MaxValue
            });
        }

        private async Task<string> GenerateTransferCodeAsync(CancellationToken cancellationToken)
            => $"TRF-{DateTime.UtcNow:yyyyMMdd}-{(await _unitOfWork.SupplyTransfers.CountTodayAsync(cancellationToken)) + 1:D3}";

        private static string BuildNotes(string reason, string? notes)
            => string.IsNullOrWhiteSpace(notes) ? $"Reason: {reason.Trim()}" : $"Reason: {reason.Trim()} | Notes: {notes.Trim()}";

        private static string AppendNotes(string? current, string? extra)
        {
            if (string.IsNullOrWhiteSpace(extra)) return current ?? string.Empty;
            if (string.IsNullOrWhiteSpace(current)) return extra.Trim();
            return $"{current}\n{extra.Trim()}";
        }

        private static List<string> NormalizeEvidenceUrls(IEnumerable<string>? urls)
            => urls?
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Select(url => url.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList()
               ?? [];

        private static List<string> MergeEvidenceUrls(IEnumerable<string>? current, IEnumerable<string>? incoming)
            => NormalizeEvidenceUrls((current ?? []).Concat(incoming ?? []));

        private static bool IsActiveAssignment(SupplyTransferVehicle assignment)
            => assignment.Status is SupplyTransferVehicleStatus.Assigned
                or SupplyTransferVehicleStatus.InTransit
                or SupplyTransferVehicleStatus.Arrived
                or SupplyTransferVehicleStatus.Incident;

        private static string NormalizeRequiredUrl(string value)
        {
            var normalized = value?.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
                throw new InvalidOperationException("FileUrl is required.");

            return normalized;
        }

        private static string? NormalizeOptional(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            var normalized = value.Trim();
            return normalized.Length <= maxLength
                ? normalized
                : normalized[..maxLength];
        }

        private static List<SupplyTransferDocumentResponse> MapDocuments(IEnumerable<SupplyTransferDocument>? documents)
            => documents?
                .OrderBy(d => d.DocumentType)
                .ThenByDescending(d => d.Version)
                .Select(d => new SupplyTransferDocumentResponse
                {
                    SupplyTransferDocumentId = d.SupplyTransferDocumentId,
                    DocumentType = d.DocumentType,
                    Version = d.Version,
                    FileUrl = d.FileUrl,
                    FileName = d.FileName,
                    ContentType = d.ContentType,
                    FileSizeBytes = d.FileSizeBytes,
                    IsCurrent = d.IsCurrent,
                    CreatedBy = d.CreatedBy,
                    CreatedAt = d.CreatedAt,
                    Notes = d.Notes
                })
                .ToList()
               ?? [];

        private static string? GetCurrentDocumentUrl(SupplyTransfer transfer, SupplyTransferDocumentType documentType)
            => transfer.Documents
                .Where(d => d.DocumentType == documentType && d.IsCurrent)
                .OrderByDescending(d => d.Version)
                .Select(d => d.FileUrl)
                .FirstOrDefault();

        private static SupplyTransferResponse MapToResponse(SupplyTransfer transfer) => new()
        {
            SupplyTransferId = transfer.SupplyTransferId,
            TransferCode = transfer.TransferCode,
            SourceStationId = transfer.SourceStationId,
            SourceStationName = transfer.SourceStation?.Name ?? string.Empty,
            DestinationStationId = transfer.DestinationStationId,
            DestinationStationName = transfer.DestinationStation?.Name ?? string.Empty,
            Status = transfer.Status,
            RequestedAt = transfer.RequestedAt,
            ApprovedAt = transfer.ApprovedAt,
            ShippedAt = transfer.ShippedAt,
            ReceivedAt = transfer.ReceivedAt,
            RequestedBy = transfer.RequestedBy,
            RequestedByName = transfer.RequestedByUser?.DisplayName ?? transfer.RequestedByUser?.UserName ?? transfer.RequestedByUser?.Email ?? string.Empty,
            ApprovedBy = transfer.ApprovedBy,
            ApprovedByName = transfer.ApprovedByUser?.DisplayName ?? transfer.ApprovedByUser?.UserName ?? transfer.ApprovedByUser?.Email,
            VehicleId = transfer.VehicleId,
            DriverUserId = transfer.DriverUserId,
            Vehicles = transfer.SupplyTransferVehicles.Select(v => new SupplyTransferVehicleResponse
            {
                SupplyTransferVehicleId = v.SupplyTransferVehicleId,
                VehicleId = v.VehicleId,
                LicensePlate = v.Vehicle?.LicensePlate ?? string.Empty,
                VehicleTypeId = v.Vehicle?.VehicleTypeId ?? Guid.Empty,
                VehicleTypeName = v.Vehicle?.VehicleType?.TypeName ?? string.Empty,
                DriverUserId = v.DriverUserId,
                DriverName = v.DriverUser?.DisplayName ?? v.DriverUser?.UserName ?? v.DriverUser?.Email,
                Status = v.Status,
                AssignedAt = v.AssignedAt,
                DepartedAt = v.DepartedAt,
                ArrivedAt = v.ArrivedAt,
                CompletedAt = v.CompletedAt,
                Note = v.Note
            }).ToList(),
            Notes = transfer.Notes,
            EvidenceUrls = transfer.EvidenceUrls,
            Documents = MapDocuments(transfer.Documents),
            CurrentRequestPdfUrl = GetCurrentDocumentUrl(transfer, SupplyTransferDocumentType.RequestPdf),
            CurrentConfirmedPdfUrl = GetCurrentDocumentUrl(transfer, SupplyTransferDocumentType.ConfirmedPdf),
            InventoryTransactionIds = transfer.InventoryTransactions.Select(t => t.TransactionId).ToList(),
            Items = transfer.Items.Select(i => new SupplyTransferItemResponse
            {
                SupplyTransferItemId = i.SupplyTransferItemId,
                SupplyItemId = i.SupplyItemId,
                SupplyItemName = i.SupplyItem?.Name ?? string.Empty,
                RequestedQuantity = i.RequestedQuantity,
                ActualQuantity = i.ActualQuantity,
                Notes = i.Notes
            }).ToList()
        };

        private static SupplyTransferSummaryResponse MapToSummary(SupplyTransfer transfer) => new()
        {
            SupplyTransferId = transfer.SupplyTransferId,
            TransferCode = transfer.TransferCode,
            SourceStationId = transfer.SourceStationId,
            SourceStationName = transfer.SourceStation?.Name ?? string.Empty,
            DestinationStationId = transfer.DestinationStationId,
            DestinationStationName = transfer.DestinationStation?.Name ?? string.Empty,
            Status = transfer.Status,
            RequestedAt = transfer.RequestedAt,
            RequestedByName = transfer.RequestedByUser?.DisplayName ?? transfer.RequestedByUser?.UserName ?? transfer.RequestedByUser?.Email ?? string.Empty,
            TotalRequestedItems = transfer.Items.Count,
            TotalRequestedQuantity = transfer.Items.Sum(i => i.RequestedQuantity),
            Notes = transfer.Notes,
            EvidenceUrls = transfer.EvidenceUrls,
            CurrentRequestPdfUrl = GetCurrentDocumentUrl(transfer, SupplyTransferDocumentType.RequestPdf),
            CurrentConfirmedPdfUrl = GetCurrentDocumentUrl(transfer, SupplyTransferDocumentType.ConfirmedPdf)
        };
    }
}
