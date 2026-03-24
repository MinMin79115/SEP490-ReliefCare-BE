using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests
{
    public class ChangeCampaignStatusRequest
    {
        [Required]
        public CampaignStatus Status { get; set; }
    }
}
