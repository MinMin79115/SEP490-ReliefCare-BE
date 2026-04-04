using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum CampaignType
    {
        /// <summary>Chiến dịch kêu gọi nguồn lực (tiền/vật tư/con người).</summary>
        Fundraising = 1,

        /// <summary>Chiến dịch thực thi cứu trợ/phân phát.</summary>
        Relief = 2,

        /// <summary>Chiến dịch cứu hộ khẩn cấp trong vùng thiên tai.</summary>
        Rescue = 3
    }
}
