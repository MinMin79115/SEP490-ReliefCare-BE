using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.StationJoinRequest.DTOs.Request
{
    public class ReviewStationJoinRequestRequest
    {
        public string? ReviewNote { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }
    }
}
