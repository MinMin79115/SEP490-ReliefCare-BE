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
            : base("Không thể tìm thấy tỉnh này",
                "LOCATION_NOT_FOUND",
                404)
        {
        }
    }
}
