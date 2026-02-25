using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Inventory.DTOs.Response
{
    /// <summary>
    /// Summary response for an inventory (used in list views).
    /// </summary>
    public class InventoryResponse
    {
        public Guid InventoryId { get; set; }
        public Guid ReliefStationId { get; set; }
        public string ReliefStationName { get; set; } = null!;
        public InventoryLevel Level { get; set; }
        public string LevelName => Level.ToString();
        public EntityStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public int TotalStockSlots { get; set; }
        public int CriticalCount { get; set; }
    }

    /// <summary>
    /// Detail response for an inventory including all stock items.
    /// </summary>
    public class InventoryDetailResponse : InventoryResponse
    {
        public IReadOnlyList<InventoryStockResponse> Stocks { get; set; } = [];
    }
}
