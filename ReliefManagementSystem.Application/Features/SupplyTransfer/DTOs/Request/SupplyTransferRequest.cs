using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.SupplyTransfer.DTOs.Request
{
    public class CreateSupplyTransferRequest
    {
        [Required] public Guid SourceStationId { get; set; }
        [Required] public Guid DestinationStationId { get; set; }
        [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
        [MaxLength(1000)] public string? Notes { get; set; }
        public List<string> EvidenceUrls { get; set; } = [];
        [Required, MinLength(1)] public List<CreateSupplyTransferItemRequest> Items { get; set; } = [];
    }

    public class CreateSupplyTransferItemRequest
    {
        [Required] public Guid SupplyItemId { get; set; }
        [Range(1, int.MaxValue)] public int Quantity { get; set; }
        [MaxLength(500)] public string? Notes { get; set; }
    }

    public class ApproveSupplyTransferRequest
    {
        [MaxLength(1000)] public string? Notes { get; set; }
        public List<string> EvidenceUrls { get; set; } = [];
    }

    public class ShipSupplyTransferRequest
    {
        public Guid? VehicleId { get; set; }
        public Guid? DriverUserId { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
        public List<string> EvidenceUrls { get; set; } = [];
    }

    public class ReceiveSupplyTransferRequest
    {
        [Required, MinLength(1)] public List<ReceiveSupplyTransferItemRequest> Items { get; set; } = [];
        [MaxLength(1000)] public string? Notes { get; set; }
        public List<string> EvidenceUrls { get; set; } = [];
    }

    public class ReceiveSupplyTransferItemRequest
    {
        [Required] public Guid SupplyItemId { get; set; }
        [Range(0, int.MaxValue)] public int ActualQuantity { get; set; }
        [MaxLength(500)] public string? Notes { get; set; }
    }

    public class CancelSupplyTransferRequest
    {
        [MaxLength(1000)] public string? Notes { get; set; }
        public List<string> EvidenceUrls { get; set; } = [];
    }

    public class ReplaceSupplyTransferEvidenceUrlsRequest
    {
        public List<string> EvidenceUrls { get; set; } = [];
    }

    public class AppendSupplyTransferEvidenceUrlsRequest
    {
        public List<string> EvidenceUrls { get; set; } = [];
    }

    public class CreateSupplyTransferDocumentRequest
    {
        [Required]
        public SupplyTransferDocumentType DocumentType { get; set; }

        [Required]
        [MaxLength(2000)]
        public string FileUrl { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? FileName { get; set; }

        [MaxLength(100)]
        public string? ContentType { get; set; }

        public long? FileSizeBytes { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
