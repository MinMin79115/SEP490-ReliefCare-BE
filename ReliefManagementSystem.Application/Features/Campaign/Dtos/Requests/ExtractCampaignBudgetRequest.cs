using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests
{
    public class ExtractCampaignBudgetRequest
    {
        [Required]
        public Guid TargetReliefCampaignId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [MaxLength(1000)]
        public string? Note { get; set; }
    }
}
