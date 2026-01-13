using Microsoft.AspNetCore.Identity;
using ReliefManagementSystem.Application.Features.Auth.DTOs;
using ReliefManagementSystem.Application.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IIdentityAuthService _identityAuthService;
        private readonly ITokenService _tokenService;

        public AuthService(
            IIdentityAuthService identityAuthService,
            ITokenService tokenService)
        {
            _identityAuthService = identityAuthService;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken)
        {
            var user = await _identityAuthService.RegisterAsync(
                request,
                cancellationToken);

            var token = await _tokenService.GenerateTokenAsync(
                user,
                new[] { "api" },
                cancellationToken);

            return new AuthResponse
            {
                UserId = user.Id,
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                AccessTokenExpires = token.AccessTokenExpires
            };
        }

        public async Task<AuthResponse> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            var user = await _identityAuthService.ValidateByEmailAsync(
                request.Email,
                request.Password,
                cancellationToken);

            var token = await _tokenService.GenerateTokenAsync(
                user,
                new[] { "api" },
                cancellationToken);

            return new AuthResponse
            {
                UserId = user.Id,
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                AccessTokenExpires = token.AccessTokenExpires
            };
        }

        public async Task<AuthResponse> LoginPhoneAsync(
            LoginPhoneRequest request,
            CancellationToken cancellationToken)
        {
            var user = await _identityAuthService.ValidateByPhoneAsync(
                request.PhoneNumber,
                request.Password,
                cancellationToken);

            var token = await _tokenService.GenerateTokenAsync(
                user,
                new[] { "api" },
                cancellationToken);

            return new AuthResponse
            {
                UserId = user.Id,
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                AccessTokenExpires = token.AccessTokenExpires
            };
        }

        public async Task<AuthResponse> LoginGoogleAsync(CancellationToken cancellationToken)
        {
           var user = await _identityAuthService.ValidateByGoogleAsync(cancellationToken);
            var token = await _tokenService.GenerateTokenAsync(
                user,
                new[] { "api" },
                CancellationToken.None);
            return new AuthResponse
            {
                UserId = user.Id,
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                AccessTokenExpires = token.AccessTokenExpires
            };
        }
    }
}
