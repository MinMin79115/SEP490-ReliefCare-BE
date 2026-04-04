using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class DuplicateReliefStationLocationException : AppException
    {
        public DuplicateReliefStationLocationException()
            : base("Đã có trạm tồn tại ở tỉnh này",
                "PROVINCIAL_STATION_ALREADY_EXISTS",
                400)
        {
        }
    }
}
