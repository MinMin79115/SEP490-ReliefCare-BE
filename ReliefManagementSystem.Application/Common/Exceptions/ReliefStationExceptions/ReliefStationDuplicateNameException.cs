using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ReliefStationDuplicateNameException : AppException
    {
        public ReliefStationDuplicateNameException()
            : base("Relief station name already exists",
                "RELIEF_STATION_DUPLICATE_NAME",
                409)
        {
        }
    }
}
