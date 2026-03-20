namespace ReliefManagementSystem.Domain.Enum
{
    public enum ModeratorStatus
    {
        /// <summary>Đang hoạt động bình thường tại trạm.</summary>
        Active = 1,

        /// <summary>Chưa được gán trạm hoặc đang rảnh.</summary>
        Inactive = 2,

        /// <summary>Đang bị đình chỉ tạm thời.</summary>
        Suspended = 3,

        /// <summary>Đã bị sa thải hoặc tước quyền Moderator.</summary>
        Dismissed = 4
    }
}
