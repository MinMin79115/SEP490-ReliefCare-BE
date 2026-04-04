using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class StationNameAlreadyExistsException : AppException
    {
        public StationNameAlreadyExistsException(string name)
            : base($"Trạm bị trùng tên vui lòn xử dụng tên khác",
                "RELIEF_STATION_NAME_EXISTS",
                400)
        {
        }
    }
}
