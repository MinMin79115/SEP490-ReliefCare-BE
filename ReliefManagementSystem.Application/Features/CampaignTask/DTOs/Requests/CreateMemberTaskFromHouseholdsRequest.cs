using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests
{
    public class CreateMemberTaskFromHouseholdsRequest
    {
        [Required]
        public Guid VolunteerProfileId { get; set; }

        [Required]
        [MinLength(1)]
        public List<Guid> HouseholdDeliveryIds { get; set; } = [];

        [MaxLength(200)]
        public string? SubTaskTitle { get; set; }

        [MaxLength(1000)]
        public string? TaskNote { get; set; }
    }
}
