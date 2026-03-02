using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.Auth.DTOs
{
    public class ChangePasswordRequest
    { 
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
