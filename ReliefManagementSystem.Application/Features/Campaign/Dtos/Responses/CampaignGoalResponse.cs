using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Responses
{
    public class CampaignGoalResponse
    {
        public Guid CampaignResourceGoalId { get; set; }
        public CampaignResourceType ResourceType { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public bool IsRequired { get; set; }
        public bool IsMet { get; set; }
        public decimal ProgressPercent { get; set; }
    }
}
