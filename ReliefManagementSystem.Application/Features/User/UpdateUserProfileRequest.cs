using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ReliefManagementSystem.Application.Features.User
{
    public class UpdateUserProfileRequest
    {
        public string? DisplayName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
