using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum TransactionReason
    {
        Donation = 1,           // Nhập kho từ quyên góp (InKindDonation)
        SupplyTransferIn = 2,   // Nhập kho từ một trạm khác chuyển đến
        SupplyTransferOut = 3,  // Xuất kho để chuyển đến trạm khác
        CampaignAllocation = 4, // Xuất kho để cấp phát cho chiến dịch
        Other = 5,              // Các lý do khác (VD: kiểm kê, hư hỏng, v.v.)
        Procurement = 6,        // Nhập kho từ mua sắm nội bộ bằng ngân sách campaign
        PackageAssemblyConsume = 7, // Xuất kho vật tư thành phần để đóng gói
        PackageAssemblyProduce = 8  // Nhập kho thành phẩm gói cứu trợ
    }
}
