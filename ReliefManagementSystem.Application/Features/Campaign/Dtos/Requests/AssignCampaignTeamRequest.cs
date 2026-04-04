using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests
{
    public class AssignCampaignTeamRequest
    {
        [Required]
        public Guid TeamId { get; set; }

        [Required]
        public CampaignTeamRole Role { get; set; }

        public CampaignTeamStatus InitialStatus { get; set; } = CampaignTeamStatus.Active;
    }
}
