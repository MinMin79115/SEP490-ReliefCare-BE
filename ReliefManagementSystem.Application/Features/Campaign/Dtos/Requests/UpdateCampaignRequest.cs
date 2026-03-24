using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests
{
    public class UpdateCampaignRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double AreaRadiusKm { get; set; }
        public string? AddressDetail { get; set; }

        public bool AllowOverTarget { get; set; } = true;
        public CampaignCompletionRule CompletionRule { get; set; } = CampaignCompletionRule.RequiredGoalsMet;
    }
}
