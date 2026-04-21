using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.InventoryTransaction.DTOs.Response
{
    /// <summary>
    /// Full detail response for a transaction including all line items.
    /// Used for single transaction view.
    /// </summary>
    public class TransactionResponse
    {
        public Guid TransactionId { get; set; }
        public Guid InventoryId { get; set; }
        public string ReliefStationName { get; set; } = null!;
        public string TransactionCode { get; set; } = null!;
        public TransactionType Type { get; set; }
        public string TypeName => Type.ToString();
        public TransactionReason Reason { get; set; }
        public string ReasonName => Reason.ToString();
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public string CreatedByName { get; set; } = null!;
        public string? ImportBatchCode { get; set; }
        public string? SourceReference { get; set; }
        public string? Notes { get; set; }
        public IReadOnlyList<TransactionItemResponse> Items { get; set; } = [];
    }

    /// <summary>One resolved line item in a transaction response.</summary>
    public class TransactionItemResponse
    {
        public Guid TransactionItemId { get; set; }
        public Guid SupplyItemId { get; set; }
        public string SupplyItemName { get; set; } = null!;
        public string SupplyItemUnit { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal? UnitCost { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Lightweight summary for list views — excludes line items.
    /// </summary>
    public class TransactionSummaryResponse
    {
        public Guid TransactionId { get; set; }
        public Guid InventoryId { get; set; }
        public string TransactionCode { get; set; } = null!;
        public TransactionType Type { get; set; }
        public string TypeName => Type.ToString();
        public TransactionReason Reason { get; set; }
        public string ReasonName => Reason.ToString();
        public int TotalItems { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByName { get; set; } = null!;
        public string? ImportBatchCode { get; set; }
        public string? SourceReference { get; set; }
        public string? Notes { get; set; }
    }
}
