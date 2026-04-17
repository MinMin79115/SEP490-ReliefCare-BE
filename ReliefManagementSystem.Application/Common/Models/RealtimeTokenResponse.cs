using System;

namespace ReliefManagementSystem.Application.Common.Models
{
    public class RealtimeTokenResponse
    {
        public string Token { get; set; } = null!;
        public string Endpoint { get; set; } = null!;
        public string Channel { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
