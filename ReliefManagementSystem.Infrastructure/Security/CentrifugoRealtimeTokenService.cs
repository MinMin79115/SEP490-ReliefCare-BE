using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System;

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
                Endpoint = BuildWebsocketEndpoint(_centrifugoSettings.PublicWebsocketUrl),
                Channel = channel,
                ExpiresAt = expiresAt
            });
        }

        private static string GetNotificationChannel(Guid userId)
            => $"notifications_user_{userId}";

        private static string BuildWebsocketEndpoint(string configuredUrl)
        {
            if (string.IsNullOrWhiteSpace(configuredUrl))
            {
                throw new InvalidOperationException("Centrifugo PublicWebsocketUrl is not configured.");
            }

            var trimmed = configuredUrl.Trim();

            if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            {
                throw new InvalidOperationException("Centrifugo PublicWebsocketUrl must be an absolute URL.");
            }

            var scheme = uri.Scheme switch
            {
                "http" => "ws",
                "https" => "wss",
                "ws" => "ws",
                "wss" => "wss",
                _ => throw new InvalidOperationException("Centrifugo PublicWebsocketUrl must use http, https, ws, or wss scheme.")
            };

            var path = uri.AbsolutePath.TrimEnd('/');
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                path = "/connection/websocket";
            }
            else if (!path.EndsWith("/connection/websocket", StringComparison.OrdinalIgnoreCase))
            {
                path += "/connection/websocket";
            }

            var builder = new UriBuilder(uri)
            {
                Scheme = scheme,
                Path = path,
                Query = string.Empty
            };

            return builder.Uri.AbsoluteUri;
        }
    }
}
