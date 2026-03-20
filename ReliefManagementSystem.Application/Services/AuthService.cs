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
    }
}
