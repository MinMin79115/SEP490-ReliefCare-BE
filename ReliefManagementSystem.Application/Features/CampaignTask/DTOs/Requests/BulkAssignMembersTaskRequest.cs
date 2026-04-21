using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests
{
    public class BulkAssignMembersTaskRequest
    {
        [Required]
        [MinLength(1)]
        public List<AssignMemberTaskRequest> Members { get; set; } = new();
    }
}