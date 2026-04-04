using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ReliefStationCreationFailedException :AppException
    {
        public ReliefStationCreationFailedException()
            : base("Lỗi trong quá trình tạo trạm",
                "ReliefStation_CREATION_FAILED",
                500)
        {
        }
    }
}
