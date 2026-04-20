using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests
{
    public class CreateCampaignTaskRequest
    {
        [Required]
        public Guid CampaignTeamId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    }
}
