using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class CampaignResourceGoal
    {
        public Guid CampaignResourceGoalId { get; set; }

        public Guid CampaignId { get; set; }
        public Campaign Campaign { get; set; } = null!;

        public CampaignResourceType ResourceType { get; set; }

        /// <summary>
        /// Mục tiêu cho resource type tương ứng.
        /// Money: số tiền, Supplies: tổng số lượng vật tư quy đổi, People: số người cần thêm.
        /// </summary>
        public decimal TargetAmount { get; set; }

        /// <summary>
        /// Số lượng đã đạt được tới hiện tại.
        /// </summary>
        public decimal ReceivedAmount { get; set; }

        public bool IsMet { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
