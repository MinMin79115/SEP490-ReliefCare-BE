namespace ReliefManagementSystem.Application.Features.Donation.DTOs.Response
{
    public class AdminDonationStatsResponse
    {
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int CompletedCount { get; set; }
        public int FailedCount { get; set; }
        public int CancelledCount { get; set; }
        public int ExpiredCount { get; set; }
    }
}
