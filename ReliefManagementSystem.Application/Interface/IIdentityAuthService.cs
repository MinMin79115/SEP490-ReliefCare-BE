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

        Task ForgotPasswordAsync(
            string email,
            CancellationToken cancellationToken);

        Task ResetPasswordAsync(
            string email,
            string token,
            string newPassword,
            CancellationToken cancellationToken);

        /// <summary>Gửi email xác thực tới địa chỉ email vừa đăng ký.</summary>
        Task SendEmailConfirmationAsync(ApplicationUser user, CancellationToken cancellationToken);

        /// <summary>Xác nhận email từ token trong link được gửi qua email.</summary>
        Task ConfirmEmailAsync(string email, string token, CancellationToken cancellationToken);
    }
}
