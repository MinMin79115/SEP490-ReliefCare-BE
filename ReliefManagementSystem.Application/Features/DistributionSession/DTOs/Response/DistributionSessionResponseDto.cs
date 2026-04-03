using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.DistributionSession.DTOs.Response
{
    public class DistributionSessionResponseDto
    {
        public Guid DistributionSessionId { get; set; }
        public Guid CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public Guid ReliefStationId { get; set; }
        public string ReliefStationName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DistributionSessionMode Mode { get; set; }
        public DistributionSessionStatus Status { get; set; }
        public DateTime ScheduledStartAt { get; set; }
        public DateTime? ScheduledEndAt { get; set; }
        public string? LocationName { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? RadiusMeters { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<DistributionSessionItemResponseDto> Items { get; set; } = new();
        public List<DistributionSessionRequestResponseDto> Requests { get; set; } = new();
    }

    public class DistributionSessionItemResponseDto
    {
        public Guid DistributionSessionItemId { get; set; }
        public Guid SupplyItemId { get; set; }
        public string SupplyItemName { get; set; } = string.Empty;
        public Guid? SupplyAllocationItemId { get; set; }
        public decimal ReservedQuantity { get; set; }
        public decimal DeliveredQuantity { get; set; }
    }

    public class DistributionSessionRequestResponseDto
    {
        public Guid ReliefRequestId { get; set; }
        public string ReporterFullName { get; set; } = string.Empty;
        public string ReporterPhone { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string ReliefRequestStatus { get; set; } = string.Empty;
        public string? PlannedNote { get; set; }
    }
}
