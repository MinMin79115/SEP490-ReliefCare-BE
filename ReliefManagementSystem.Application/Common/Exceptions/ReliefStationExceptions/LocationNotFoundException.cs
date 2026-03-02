using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class LocationNotFoundException : AppException
    {
        public LocationNotFoundException()
            : base("Location not found",
                "LOCATION_NOT_FOUND",
                404)
        {
        }
    }
}
