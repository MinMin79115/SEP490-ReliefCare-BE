using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum RequestStatus
    {
        Draft =0 ,      
        Submitted = 1, 
        Verified =2,
        Rejected =3 ,
        InProgress =4, 
        Resolved =5,   
        Cancelled =6
    }

}
