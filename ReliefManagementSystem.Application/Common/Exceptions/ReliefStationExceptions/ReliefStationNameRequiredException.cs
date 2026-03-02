using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ReliefStationNameRequiredException : AppException
    {
        public ReliefStationNameRequiredException()
            : base("Relief station name is required",
                "RELIEF_STATION_NAME_REQUIRED",
                400)
        {
        }
    }
}
