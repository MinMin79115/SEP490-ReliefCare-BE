using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Inventory
{
    public class BulkTransactionItemRequest
    {
        public Guid InventoryItemId { get; set; }
        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
    }

    public class BulkImportRequest
    {
        public string? Notes { get; set; }
        public List<BulkTransactionItemRequest> Items { get; set; } = new();
    }

    public class BulkExportRequest
    {
        public string? Notes { get; set; }
        public string? RecipientInfo { get; set; }
        public List<BulkTransactionItemRequest> Items { get; set; } = new();
    }

    public class ItemTransactionResult
    {
        public Guid ItemId { get; set; }
        public string ItemName { get; set; } = null!;
        public decimal PreviousQuantity { get; set; }
        public decimal TransactionQuantity { get; set; }
        public decimal CurrentQuantity { get; set; }
        public InventoryStatus Status { get; set; }
        public string StatusText { get; set; } = null!;
    }

    public class BulkTransactionResponse
    {
        public Guid BatchId { get; set; }
        public string BatchNumber { get; set; } = null!;
        public TransactionType BatchType { get; set; }
        public int TotalItems { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public string? Notes { get; set; }
        public string? RecipientInfo { get; set; }
        public List<ItemTransactionResult> Items { get; set; } = new();
    }

    public class BatchDto
    {
        public Guid BatchId { get; set; }
        public string BatchNumber { get; set; } = null!;
        public TransactionType BatchType { get; set; }
        public string BatchTypeText { get; set; } = null!;
        public int TotalItems { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public string? Notes { get; set; }
        public string? RecipientInfo { get; set; }
        public BatchStatus Status { get; set; }
    }

    public class BatchDetailDto : BatchDto
    {
        public List<TransactionItemDto> Items { get; set; } = new();
    }

    public class TransactionItemDto
    {
        public Guid TransactionId { get; set; }
        public string ItemName { get; set; } = null!;
        public string ItemCode { get; set; } = null!;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = null!;
        public string? Notes { get; set; }
    }
}
