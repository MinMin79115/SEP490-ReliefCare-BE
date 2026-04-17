using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ReliefManagementSystem.Infrastructure.Security
{
    public class CentrifugoRealtimeTokenService : IRealtimeTokenService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly CentrifugoSettings _centrifugoSettings;

        public CentrifugoRealtimeTokenService(
            ICurrentUserService currentUserService,
            IOptions<CentrifugoSettings> centrifugoOptions)
        {
            _currentUserService = currentUserService;
            _centrifugoSettings = centrifugoOptions.Value;
        }

        public Task<RealtimeTokenResponse> GenerateForCurrentUserAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedAccessException("User not authenticated.");

            var channel = GetNotificationChannel(userId);
            var expiresAt = DateTime.UtcNow.AddMinutes(_centrifugoSettings.ConnectionTokenMinutes);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_centrifugoSettings.ClientTokenSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var payload = new JwtPayload(
                issuer: _centrifugoSettings.Issuer,
                audience: _centrifugoSettings.Audience,
                claims: null,
                notBefore: DateTime.UtcNow,
                expires: expiresAt);

            payload[JwtRegisteredClaimNames.Sub] = userId.ToString();
            payload["channels"] = new[] { channel };

            var info = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(_currentUserService.Email))
            {
                info["email"] = _currentUserService.Email!;
            }

            if (!string.IsNullOrWhiteSpace(_currentUserService.DisplayName))
            {
                info["name"] = _currentUserService.DisplayName!;
            }

            if (info.Count > 0)
            {
                payload["info"] = info;
            }

            var token = new JwtSecurityToken(new JwtHeader(credentials), payload);
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Task.FromResult(new RealtimeTokenResponse
            {
                Token = tokenString,
                Endpoint = $"{_centrifugoSettings.PublicWebsocketUrl.TrimEnd('/')}/connection/websocket",
                Channel = channel,
                ExpiresAt = expiresAt
            });
        }

        private static string GetNotificationChannel(Guid userId)
            => $"notifications_user_{userId}";
    }
}
