using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Models
{
    public class JwtSettings
    {
        public string Key { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int AccessTokenMinutes { get; set; }
        public int RefreshTokenDays { get; set; }
    }

    public class CentrifugoSettings
    {
        public string BaseUrl { get; set; } = null!;
        public string PublicWebsocketUrl { get; set; } = null!;
        public string HttpApiKey { get; set; } = null!;
        public string ClientTokenSecret { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int ConnectionTokenMinutes { get; set; } = 15;
    }

}
