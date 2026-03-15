using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.Auth.DTOs
{
    public class AuthResponse
    {
        public Guid? UserId { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? AccessTokenExpires { get; set; }

        /// <summary>Thông báo phụ, ví dụ: yêu cầu xác thực email.</summary>
        public string? Message { get; set; }
    }
}
