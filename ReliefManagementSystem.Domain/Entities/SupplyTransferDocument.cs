using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    public class SupplyTransferDocument
    {
        public Guid SupplyTransferDocumentId { get; set; }

        public Guid SupplyTransferId { get; set; }

        public SupplyTransferDocumentType DocumentType { get; set; }

        [Range(1, int.MaxValue)]
        public int Version { get; set; } = 1;

        [Required]
        [MaxLength(2000)]
        public string FileUrl { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? FileName { get; set; }

        [MaxLength(100)]
        public string? ContentType { get; set; }

        public long? FileSizeBytes { get; set; }

        public bool IsCurrent { get; set; } = true;

        public Guid? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public SupplyTransfer SupplyTransfer { get; set; } = null!;
    }
}
