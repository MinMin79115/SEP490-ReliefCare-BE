using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Responses
{
    public class MemberTaskDeliveryResponse
    {
        public Guid MemberTaskDeliveryId { get; set; }
        public Guid MemberTaskId { get; set; }
        public Guid HouseholdDeliveryId { get; set; }
        public Guid CampaignHouseholdId { get; set; }
        public string HouseholdCode { get; set; } = string.Empty;
        public string HeadOfHouseholdName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public Guid? AssignedVolunteerProfileId { get; set; }
        public string? AssignedVolunteerName { get; set; }
        public MemberTaskStatus Status { get; set; }
        public HouseholdFulfillmentStatus DeliveryStatus { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Note { get; set; }
    }
}
