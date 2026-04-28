using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests
{
    public class AssignMemberTaskDeliveriesRequest
    {
        [Required]
        [MinLength(1)]
        public List<Guid> HouseholdDeliveryIds { get; set; } = [];

        public Guid? AssignedVolunteerProfileId { get; set; }
        public string? Note { get; set; }
    }
}
