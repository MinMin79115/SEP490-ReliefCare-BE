using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    /// <summary>
    /// Chứng chỉ của tình nguyện viên (được lưu sau khi hồ sơ được duyệt).
    /// </summary>
    public class VolunteerCertificate
    {
        public Guid CertificateId { get; set; }

        public Guid VolunteerProfileId { get; set; }

        /// <summary>Tên chứng chỉ, ví dụ: "Sơ cứu cơ bản", "Phòng cháy chữa cháy".</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Tổ chức / cơ quan cấp chứng chỉ.</summary>
        public string? IssuedBy { get; set; }

        /// <summary>Ngày cấp chứng chỉ.</summary>
        public DateTime? IssuedDate { get; set; }

        /// <summary>Ngày hết hạn (null = không giới hạn thời gian).</summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>URL ảnh / file chứng chỉ (Cloudinary hoặc storage khác).</summary>
        public string? FileUrl { get; set; }

        public VolunteerProfile VolunteerProfile { get; set; } = default!;
    }
}
