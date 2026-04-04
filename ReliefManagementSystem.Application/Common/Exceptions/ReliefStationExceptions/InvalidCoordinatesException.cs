using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class InvalidCoordinatesException : AppException
    {
        public InvalidCoordinatesException()
            : base("Kinh độ hoặc vĩ dộ không hợp lệ",
                "INVALID_COORDINATES",
                400)
        {
        }
    }
}
