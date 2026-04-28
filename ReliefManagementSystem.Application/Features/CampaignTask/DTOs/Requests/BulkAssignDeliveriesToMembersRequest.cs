using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests
{
    public class BulkAssignDeliveriesToMembersRequest
    {
        [Required]
        [MinLength(1)]
        public List<MemberDeliveryAssignmentGroupRequest> Assignments { get; set; } = [];
    }

    public class MemberDeliveryAssignmentGroupRequest
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

        [MaxLength(100)]
        public string? LineName { get; set; }
    }
}
