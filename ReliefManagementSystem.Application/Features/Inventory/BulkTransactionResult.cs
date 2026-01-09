namespace ReliefManagementSystem.Application.Features.Inventory
{
    public class BulkTransactionResult
    {
        public Guid TransactionId { get; set; }
        public string TransactionCode { get; set; } = null!;
        public int ItemsProcessed { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
