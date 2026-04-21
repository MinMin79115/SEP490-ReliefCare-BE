using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests
{
    public class AssignMemberTaskRequest
    {
        [Required]
        public Guid VolunteerProfileId { get; set; }

        [Required]
        [MaxLength(200)]
        public string SubTaskTitle { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? TaskNote { get; set; }
    }
}
