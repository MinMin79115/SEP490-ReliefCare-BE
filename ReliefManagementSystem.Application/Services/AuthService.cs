using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.Auth.DTOs;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            IIdentityAuthService identityAuthService,
            ITokenService tokenService,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _identityAuthService = identityAuthService;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResponse> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken)
        {
            // Tạo user và gửi OTP xác thực email
            await _identityAuthService.RegisterAsync(
                request,
                cancellationToken);

            // Không cấp JWT ngay — user phải xác thực email trước
            return new AuthResponse
            {
                Message = "Registration successful. Please check your email for 6-digit verification code."
            };
        }

        public async Task<AuthResponse> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            var identifier = request.Identifier ?? request.Email;
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new UnauthorizedAccessException("Identifier is required.");
            }

            var user = await _identityAuthService.ValidateByEmailAsync(
                identifier,
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

        public async Task ChangePasswordAsync(
            ChangePasswordRequest request,
            CancellationToken cancellationToken)
        {
            await _identityAuthService.ChangePasswordAsync(
                request.CurrentPassword,
                request.NewPassword,
                cancellationToken
            );
        }

        public async Task SendForgotPasswordOtpAsync(
            SendForgotPasswordOtpRequest request,
            CancellationToken cancellationToken)
        {
            await _identityAuthService.SendForgotPasswordOtpAsync(
                request.Email,
                cancellationToken);
        }

        public async Task<AuthResponse> VerifyForgotPasswordOtpAsync(
            VerifyForgotPasswordOtpRequest request,
            CancellationToken cancellationToken)
        {
            var resetToken = await _identityAuthService.VerifyForgotPasswordOtpAsync(
                request.Email,
                request.OtpCode,
                cancellationToken);

            return new AuthResponse
            {
                Message = "OTP verified. You can reset password now.",
                ResetToken = resetToken
            };
        }

        public async Task ResetPasswordByTokenAsync(
            ResetPasswordByTokenRequest request,
            CancellationToken cancellationToken)
        {
            await _identityAuthService.ResetPasswordByTokenAsync(
                request.Email,
                request.ResetToken,
                request.NewPassword,
                cancellationToken);
        }

        public async Task VerifyEmailOtpAsync(VerifyEmailOtpRequest request, CancellationToken cancellationToken)
        {
            await _identityAuthService.VerifyEmailOtpAsync(request.Email, request.Code, cancellationToken);
        }

        public async Task ResendEmailOtpAsync(ResendEmailOtpRequest request, CancellationToken cancellationToken)
        {
            await _identityAuthService.ResendEmailOtpAsync(request.Email, cancellationToken);
        }

        public async Task<AuthResponse> RefreshTokenAsync(
            RefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                throw new UnauthorizedAccessException("Refresh token is required.");
            }

            var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(request.RefreshToken.Trim());
            if (storedToken == null)
            {
                throw new UnauthorizedAccessException("Refresh token is invalid.");
            }

            if (!storedToken.IsActive)
            {
                throw new UnauthorizedAccessException("Refresh token is expired or revoked.");
            }

            storedToken.Revoked = DateTime.UtcNow;

            var user = storedToken.User ?? await _userManager.FindByIdAsync(storedToken.UserId.ToString());
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found for refresh token.");
            }

            var token = await _tokenService.GenerateTokenAsync(
                user,
                new[] { "api" },
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AuthResponse
            {
                UserId = user.Id,
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                AccessTokenExpires = token.AccessTokenExpires
            };
        }

        public async Task LogoutAsync(
            LogoutRequest request,
            CancellationToken cancellationToken)
        {
            var refreshTokenValue = request.RefreshToken?.Trim();

            if (!string.IsNullOrWhiteSpace(refreshTokenValue))
            {
                var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshTokenValue);
                if (storedToken != null && storedToken.Revoked == null)
                {
                    storedToken.Revoked = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                return;
            }

            var userIdClaim = _httpContextAccessor.HttpContext?.User?
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
            {
                await _unitOfWork.RefreshTokens.RevokeAllByUserIdAsync(userId);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
