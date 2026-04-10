using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.SupplyTransfer.DTOs.Response
{
    public class SupplyTransferResponse
    {
        public Guid SupplyTransferId { get; set; }
        public string TransferCode { get; set; } = string.Empty;
        public Guid SourceStationId { get; set; }
        public string SourceStationName { get; set; } = string.Empty;
        public Guid DestinationStationId { get; set; }
        public string DestinationStationName { get; set; } = string.Empty;
        public SupplyTransferStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public Guid RequestedBy { get; set; }
        public string RequestedByName { get; set; } = string.Empty;
        public Guid? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public Guid? VehicleId { get; set; }
        public Guid? DriverUserId { get; set; }
        public string? Notes { get; set; }
        public List<string> EvidenceUrls { get; set; } = [];
        public List<SupplyTransferItemResponse> Items { get; set; } = [];
        public List<Guid> InventoryTransactionIds { get; set; } = [];
    }

    public class SupplyTransferItemResponse
    {
        public Guid SupplyTransferItemId { get; set; }
        public Guid SupplyItemId { get; set; }
        public string SupplyItemName { get; set; } = string.Empty;
        public int RequestedQuantity { get; set; }
        public int? ActualQuantity { get; set; }
        public string? Notes { get; set; }
    }

    public class SupplyTransferSummaryResponse
    {
        public Guid SupplyTransferId { get; set; }
        public string TransferCode { get; set; } = string.Empty;
        public string SourceStationName { get; set; } = string.Empty;
        public string DestinationStationName { get; set; } = string.Empty;
        public SupplyTransferStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public string RequestedByName { get; set; } = string.Empty;
        public Guid SourceStationId { get; set; }
        public Guid DestinationStationId { get; set; }
        public int TotalRequestedItems { get; set; }
        public int TotalRequestedQuantity { get; set; }
        public string? Notes { get; set; }
        public List<string> EvidenceUrls { get; set; } = [];
    }
}
