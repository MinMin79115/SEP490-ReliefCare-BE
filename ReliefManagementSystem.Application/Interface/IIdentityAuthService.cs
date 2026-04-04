using ReliefManagementSystem.Application.Features.Auth.DTOs;
using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IIdentityAuthService
    {
        Task<ApplicationUser> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken);

        Task<ApplicationUser> ValidateByEmailAsync(
            string email,
            string password,
            CancellationToken cancellationToken);

        Task<ApplicationUser> ValidateByPhoneAsync(
            string phone,
            string password,
            CancellationToken cancellationToken);

        Task<ApplicationUser?> ValidateByGoogleAsync(
    CancellationToken cancellationToken);

        Task ChangePasswordAsync(
            string currentPassword,
            string newPassword,
            CancellationToken cancellationToken);

        Task SendEmailOtpAsync(ApplicationUser user, CancellationToken cancellationToken);

        Task VerifyEmailOtpAsync(string email, string code, CancellationToken cancellationToken);

        Task ResendEmailOtpAsync(string email, CancellationToken cancellationToken);

        Task SendForgotPasswordOtpAsync(string email, CancellationToken cancellationToken);

        Task<string> VerifyForgotPasswordOtpAsync(string email, string otpCode, CancellationToken cancellationToken);

        Task ResetPasswordByTokenAsync(string email, string resetToken, string newPassword, CancellationToken cancellationToken);
    }
}
