using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests
{
    public class AttachCampaignStationRequest
    {
        [Required]
        public Guid ReliefStationId { get; set; }
    }
}
