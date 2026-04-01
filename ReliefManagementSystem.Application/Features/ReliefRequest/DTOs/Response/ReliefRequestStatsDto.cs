namespace ReliefManagementSystem.Application.Features.ReliefRequest.DTOs.Response
{
    public class ReliefRequestStatsDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Verified { get; set; }
        public int Approved { get; set; }
        public int Allocated { get; set; }
        public int Delivered { get; set; }
        public int Completed { get; set; }
        public int Rejected { get; set; }
    }
}
