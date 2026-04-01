using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class DistributionSession
    {
        public Guid DistributionSessionId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid ReliefStationId { get; set; }
        public DistributionSessionMode Mode { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public string? LocationName { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? RadiusMeters { get; set; }
        public DistributionSessionStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Campaign Campaign { get; set; } = default!;
        public ReliefStation ReliefStation { get; set; } = default!;
        public ICollection<DistributionSessionItem> Items { get; set; } = new List<DistributionSessionItem>();
        public ICollection<DistributionSessionRequest> Requests { get; set; } = new List<DistributionSessionRequest>();
        public ICollection<ReliefFulfillment> ReliefFulfillments { get; set; } = new List<ReliefFulfillment>();
    }
}
