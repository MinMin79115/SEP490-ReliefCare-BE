using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ReliefStationNameTooLongException : AppException
    {
        public ReliefStationNameTooLongException()
            : base("Relief station name exceeds 255 characters",
                "RELIEF_STATION_NAME_TOO_LONG",
                400)
        {
        }
    }
}
