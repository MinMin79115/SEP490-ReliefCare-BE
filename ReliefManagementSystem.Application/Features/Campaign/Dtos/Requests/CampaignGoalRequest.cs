using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests
{
    public class CampaignGoalRequest
    {
        [Required]
        public CampaignResourceType ResourceType { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TargetAmount { get; set; }
    }
}
