using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;

namespace ReliefManagementSystem.Application.Features.Auth.DTOs
{

    public class LoginRequest
    {
        public string? Identifier { get; set; }

        // Backward compatibility for existing clients still sending "email"
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        public string Password { get; set; } = null!;
    }

}
