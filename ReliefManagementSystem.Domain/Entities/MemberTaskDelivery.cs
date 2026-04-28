using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class MemberTaskDelivery
    {
        public Guid MemberTaskDeliveryId { get; set; }
        public Guid MemberTaskId { get; set; }
        public Guid HouseholdDeliveryId { get; set; }
        public Guid? AssignedVolunteerProfileId { get; set; }
        public MemberTaskStatus Status { get; set; } = MemberTaskStatus.Assigned;
        public DateTime? CompletedAt { get; set; }
        public Guid? CompletedByUserId { get; set; }
        public string? Note { get; set; }

        public MemberTask MemberTask { get; set; } = default!;
        public HouseholdDelivery HouseholdDelivery { get; set; } = default!;
        public VolunteerProfile? AssignedVolunteerProfile { get; set; }
    }
}
