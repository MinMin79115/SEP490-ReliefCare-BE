using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ManagerNotFoundException : AppException
    {
        public ManagerNotFoundException()
            : base("Manager not found",
                "MANAGER_NOT_FOUND",
                404)
        {
        }
    }
}
