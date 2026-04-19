namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Responses
{
    public class CampaignInventoryBalanceResponse
    {
        public Guid CampaignId { get; set; }
        public Guid? CampaignInventoryId { get; set; }
        public decimal BudgetTotal { get; set; }
        public decimal BudgetSpent { get; set; }
        public decimal RemainingBudget { get; set; }
        public int DistinctSupplyItemCount { get; set; }
        public int TotalQuantity { get; set; }
        public List<CampaignInventoryBalanceItemResponse> Items { get; set; } = [];
    }

    public class CampaignInventoryBalanceItemResponse
    {
        public Guid SupplyItemId { get; set; }
        public string SupplyItemName { get; set; } = string.Empty;
        public string SupplyItemUnit { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
