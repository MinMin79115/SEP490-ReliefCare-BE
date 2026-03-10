using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class InventoryCreationFailedException : AppException
    {
        public InventoryCreationFailedException()
            : base("Lỗi trong quá trình tạo kho",
                "INVENTORY_CREATION_FAILED",
                500)
        {
        }
    }
}
