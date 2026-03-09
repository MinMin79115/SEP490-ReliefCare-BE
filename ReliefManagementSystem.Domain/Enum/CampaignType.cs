using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum CampaignType
    {
        /// <summary>Chiến dịch chỉ dùng để kêu gọi quyên góp.</summary>
        Donation = 1,

        /// <summary>Chiến dịch chỉ dùng để thực hiện cứu trợ/phân phát.</summary>
        Relief = 2,

        /// <summary>Chiến dịch bao gồm cả kêu gọi quyên góp và thực hiện cứu trợ.</summary>
        Comprehensive = 3
    }
}
