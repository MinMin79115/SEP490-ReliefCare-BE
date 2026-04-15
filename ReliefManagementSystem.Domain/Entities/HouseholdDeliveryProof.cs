namespace ReliefManagementSystem.Domain.Entities
{
    public class HouseholdDeliveryProof
    {
        public Guid HouseholdDeliveryProofId { get; set; }

        public Guid HouseholdDeliveryId { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public string? Note { get; set; }
        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
        public Guid? CapturedByUserId { get; set; }

        public HouseholdDelivery HouseholdDelivery { get; set; } = null!;
        public ApplicationUser? CapturedByUser { get; set; }
    }
}
