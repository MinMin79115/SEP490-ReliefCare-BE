using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? PictureUrl { get; set; }

        public string? PicturePublicId { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? DisplayName { get; set; }

        public string? Address { get; set; }

        public string? BanReason { get; set; }

        /// <summary>
        /// Profile quản lý (chỉ có nếu user có role Manager).
        /// Chứa cấp quản lý (Regional/Province/Local) và địa phương phụ trách.
        /// </summary>
        public ManagerProfile? ManagerProfile { get; set; }

        /// <summary>
        /// Profile điều phối (chỉ có nếu user có role Moderator).
        /// Chứa khu vực giám sát và ngày bổ nhiệm.
        /// </summary>
        public ModeratorProfile? ModeratorProfile { get; set; }

        public VolunteerProfile VolunteerProfile { get; set; }
        public ICollection<CampaignVolunteerRegistration> CampaignVolunteerRegistrations { get; set; } = new List<CampaignVolunteerRegistration>();
        public ICollection<TeamMember> TeamMembers { get; set; }

        /// <summary>
        /// Trạm cứu trợ mà user này đang quản lý (role Manager, 1 Manager – 1 trạm).
        /// Từ đây có thể lấy ManagedStation.Location để biết cấp vùng (tỉnh / huyện / xã).
        /// </summary>
        public ReliefStation? ManagedStation { get; set; }
        public ICollection<ReliefPackageAssembly> CreatedReliefPackageAssemblies { get; set; } = new List<ReliefPackageAssembly>();
    }
}
