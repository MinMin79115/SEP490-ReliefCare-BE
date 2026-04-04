namespace ReliefManagementSystem.Domain.Entities
{
    public class DistributionSessionRequest
    {
        public Guid DistributionSessionId { get; set; }
        public Guid ReliefRequestId { get; set; }
        public string? PlannedNote { get; set; }

        public DistributionSession DistributionSession { get; set; } = default!;
        public ReliefRequest ReliefRequest { get; set; } = default!;
    }
}
