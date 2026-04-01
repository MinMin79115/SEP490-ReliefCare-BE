using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.ReliefRequest.DTOs.Request
{
    public class AssignReliefRequestCampaignDto
    {
        [Required]
        public Guid CampaignId { get; set; }
    }
}
