using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests
{
    public class UpdateCampaignTeamStatusRequest
    {
        [Required]
        public CampaignTeamStatus Status { get; set; }
    }
}
