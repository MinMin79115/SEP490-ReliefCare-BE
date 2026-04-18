namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Responses
{
    public class CampaignInventoryBalanceResponse
    {
        public Guid CampaignId { get; set; }
        public decimal BudgetTotal { get; set; }
        public decimal BudgetSpent { get; set; }
        public decimal RemainingBudget { get; set; }
        public List<CampaignInventoryBalanceStationResponse> Stations { get; set; } = [];
    }

    public class CampaignInventoryBalanceStationResponse
    {
        public Guid ReliefStationId { get; set; }
        public string ReliefStationName { get; set; } = string.Empty;
        public Guid? InventoryId { get; set; }
        public bool HasActiveInventory { get; set; }
        public int DistinctSupplyItemCount { get; set; }
        public int TotalQuantity { get; set; }
    }
}
