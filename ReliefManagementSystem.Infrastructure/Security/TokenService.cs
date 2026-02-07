using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Security
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwt;

        public TokenService(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IOptions<JwtSettings> jwtOptions)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _jwt = jwtOptions.Value;
        }
        public async Task<TokenResult> GenerateTokenAsync(ApplicationUser user, string[] scopes, CancellationToken cancellationToken)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim("scope", string.Join(" ", scopes))
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwt.Key));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var accessTokenExpires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);

            var accessToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: accessTokenExpires,
                signingCredentials: creds);

            var accessTokenString =
                new JwtSecurityTokenHandler().WriteToken(accessToken);

            var refreshTokenString = GenerateSecureRefreshToken();
            var refreshTokenExpires = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenString,
                Expires = refreshTokenExpires,
                Created = DateTime.UtcNow
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new TokenResult
            {
                UserId = user.Id,
                AccessToken = accessTokenString,
                RefreshToken = refreshTokenString,
                AccessTokenExpires = accessTokenExpires,
                RefreshTokenExpires = refreshTokenExpires
            };
        }

        private static string GenerateSecureRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
