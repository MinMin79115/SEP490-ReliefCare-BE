using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Responses
{
    public class PublicCampaignSummaryResponse
    {
        public Guid CampaignId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CampaignType Type { get; set; }
        public CampaignStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal TotalMoneyReceived { get; set; }
        public decimal TotalMoneySpent { get; set; }
        public decimal RemainingBudget { get; set; }

        public decimal PeopleTarget { get; set; }
        public decimal PeopleReached { get; set; }

        public int ProcurementOrderCount { get; set; }
        public int ProcurementReceivedCount { get; set; }
        public decimal ProcurementEstimatedTotal { get; set; }
        public decimal ProcurementActualTotal { get; set; }

        public int TotalSuppliesPurchasedUnits { get; set; }
        public int TotalSuppliesAllocatedUnits { get; set; }

        public List<CampaignGoalResponse> Goals { get; set; } = new();
    }
}
