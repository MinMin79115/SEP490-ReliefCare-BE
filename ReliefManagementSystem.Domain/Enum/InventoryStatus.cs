namespace ReliefManagementSystem.Domain.Enum
{
    public enum InventoryStatus
    {
        Critical = 1,       // < 15% - Nguy cấp
        NeedRestock = 2,    // 15-50% - Cần bổ sung
        Safe = 3,           // 50-99% - An toàn
        Full = 4            // 100% - Đầy kho
    }
}
