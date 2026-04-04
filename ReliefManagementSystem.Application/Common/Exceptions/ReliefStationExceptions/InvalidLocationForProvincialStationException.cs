using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class InvalidLocationForProvincialStationException : AppException
    {
        public InvalidLocationForProvincialStationException()
            : base("Nơi đặt trạm phải là tỉnh hoặc thành phố",
                "INVALID_PROVINCIAL_LOCATION",
                400)
        {
        }
    }
}
