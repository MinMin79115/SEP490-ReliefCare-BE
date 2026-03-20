using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests
{
    public class CreateCampaignRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public Guid LocationId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double AreaRadiusKm { get; set; }
        public string? AddressDetail { get; set; }

        [Required]
        public CampaignType Type { get; set; }

        public CampaignCompletionRule CompletionRule { get; set; } = CampaignCompletionRule.RequiredGoalsMet;

        public bool AllowOverTarget { get; set; } = true;

        /// <summary>
        /// Danh sách mục tiêu tài nguyên.
        /// - Fundraising: chọn 1..3 loại mục tiêu.
        /// - Relief/Rescue: có thể để trống ở MVP.
        /// </summary>
        public List<CampaignGoalRequest> Goals { get; set; } = new();

        /// <summary>
        /// Số người hệ thống hiện có sẵn cho campaign (để quy đổi People target còn thiếu).
        /// Dùng khi request có goal People.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int AvailablePeopleCount { get; set; } = 0;

        /// <summary>
        /// Optional: tạo campaign và gắn 1 trạm ngay.
        /// </summary>
        public Guid? ReliefStationId { get; set; }
    }
}
