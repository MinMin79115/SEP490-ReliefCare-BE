using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using ReliefManagementSystem.Application.Common.Exceptions;
using ReliefManagementSystem.Application.Common.Exceptions.Auth;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.Auth.DTOs;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Security
{
    public class IdentityAuthService : IIdentityAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public IdentityAuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ICurrentUserService currentUserService,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _currentUserService = currentUserService;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<ApplicationUser> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken)
        {
            var user = new ApplicationUser
            {
                Email = request.Email,
                UserName = request.UserName,
                PhoneNumber = request.PhoneNumber,
                DisplayName = request.FullName,
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = ConvertErrors(result.Errors);
                throw new ValidationException(errors);
            }

            await _userManager.AddToRoleAsync(user, Role.User.ToString());

            // Gửi email xác thực sau khi tạo tài khoản thành công
            await SendEmailConfirmationAsync(user, cancellationToken);

            return user;
        }

        /// <inheritdoc/>
        public async Task SendEmailConfirmationAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

            var frontendUrl = _configuration["FrontendUrl"] ?? "https://reliefcare.app";
            var confirmLink = $"{frontendUrl}/confirm-email?email={Uri.EscapeDataString(user.Email!)}&token={encodedToken}";

            var subject = "Xác thực tài khoản - Relief Care";
            var body = BuildEmailConfirmationBody(user.DisplayName ?? user.Email!, confirmLink);

            await _emailService.SendEmailAsync(user.Email!, subject, body);
        }

        /// <inheritdoc/>
        public async Task ConfirmEmailAsync(string email, string token, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email)
                ?? throw new InvalidCredentialsException();

            if (user.EmailConfirmed)
                return; // Đã xác thực rồi thì bỏ qua

            var decodedBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedBytes);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                var errors = ConvertErrors(result.Errors);
                throw new ValidationException(errors);
            }
        }

        public async Task<ApplicationUser> ValidateByEmailAsync(
            string email,
            string password,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new InvalidCredentialsException();

            if (await _userManager.IsLockedOutAsync(user))
                throw new UserLockedException();

            // Kiểm tra email đã được xác thực chưa
            if (!user.EmailConfirmed)
                throw new EmailNotConfirmedException();

            var roles = await _userManager.GetRolesAsync(user);

            var check = await _signInManager
                .CheckPasswordSignInAsync(user, password, false);

            if (!check.Succeeded)
                throw new InvalidCredentialsException();

            return user;
        }

        public async Task<ApplicationUser> ValidateByPhoneAsync(
            string phone,
            string password,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(phone);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            var check = await _signInManager
                .CheckPasswordSignInAsync(user, password, false);

            if (!check.Succeeded)
                throw new UnauthorizedAccessException("Invalid credentials");

            return user;
        }

        public async Task<ApplicationUser?> ValidateByGoogleAsync(
            CancellationToken cancellationToken)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
                return null;

            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false
            );

            ApplicationUser user;

            if (signInResult.Succeeded)
            {
                user = await _userManager.FindByLoginAsync(
                    info.LoginProvider,
                    info.ProviderKey
                );
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    ?? throw new Exception("Google email not found");

                var name = info.Principal.FindFirstValue(ClaimTypes.Name);

                user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        DisplayName = name
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                        throw new Exception(string.Join("; ",
                            createResult.Errors.Select(e => e.Description)));

                    await _userManager.AddToRoleAsync(
                        user, Role.User.ToString());
                }

                var loginResult = await _userManager.AddLoginAsync(user, info);
                if (!loginResult.Succeeded)
                    throw new Exception("Failed to link Google login");
            }

            return user;
        }

        public async Task ChangePasswordAsync(
            string currentPassword,
            string newPassword,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_currentUserService.UserId.ToString()))
                throw new UnauthorizedAccessException();

            var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());

            if (user == null)
                throw new InvalidCredentialsException();

            if (await _userManager.IsLockedOutAsync(user))
                throw new UserLockedException();

            if (currentPassword == newPassword)
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "Password", new[] { "New password must be different from current password." } }
                });

            var result = await _userManager.ChangePasswordAsync(
                user,
                currentPassword,
                newPassword
            );

            if (!result.Succeeded)
            {
                var errors = ConvertErrors(result.Errors);
                throw new ValidationException(errors);
            }

            await _userManager.UpdateSecurityStampAsync(user);
        }

        public async Task ForgotPasswordAsync(
            string email,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);

            // Không leak thông tin: nếu email không tồn tại thì vẫn trả về thành công
            if (user == null)
                return;

            var rawToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

            // TODO: Thay bằng URL frontend thực tế (có thể đọc từ appsettings["FrontendUrl"])
            var resetLink = $"https://reliefcare.app/reset-password?email={Uri.EscapeDataString(email)}&token={encodedToken}";

            var subject = "Đặt lại mật khẩu - Relief Care";
            var body = BuildForgotPasswordEmailBody(user.DisplayName ?? email, resetLink);

            await _emailService.SendEmailAsync(email, subject, body);
        }

        public async Task ResetPasswordAsync(
            string email,
            string token,
            string newPassword,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new InvalidCredentialsException();

            // Decode token từ Base64Url trước khi truyền vào UserManager
            var decodedBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedBytes);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);

            if (!result.Succeeded)
            {
                var errors = ConvertErrors(result.Errors);
                throw new ValidationException(errors);
            }

            await _userManager.UpdateSecurityStampAsync(user);
        }

        private static string BuildEmailConfirmationBody(string displayName, string confirmLink)
        {
            return $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Xác thực tài khoản</title>
  <style>
    body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f4f6f8; margin: 0; padding: 0; }}
    .wrapper {{ max-width: 560px; margin: 40px auto; background: #ffffff; border-radius: 12px;
                box-shadow: 0 4px 20px rgba(0,0,0,0.08); overflow: hidden; }}
    .header {{ background: linear-gradient(135deg, #1b5e20, #43a047); padding: 32px 40px; text-align: center; }}
    .header h1 {{ color: #ffffff; margin: 0; font-size: 22px; letter-spacing: 0.5px; }}
    .body {{ padding: 32px 40px; color: #333; }}
    .body p {{ line-height: 1.7; margin: 0 0 16px; }}
    .btn-wrap {{ text-align: center; margin: 28px 0; }}
    .btn {{ display: inline-block; background: linear-gradient(135deg, #1b5e20, #43a047);
            color: #ffffff !important; text-decoration: none; padding: 14px 36px;
            border-radius: 8px; font-weight: bold; font-size: 15px; letter-spacing: 0.3px; }}
    .note {{ font-size: 13px; color: #888; border-top: 1px solid #eee; padding-top: 16px; margin-top: 8px; }}
    .footer {{ background: #f4f6f8; text-align: center; padding: 16px; font-size: 12px; color: #aaa; }}
  </style>
</head>
<body>
  <div class=""wrapper"">
    <div class=""header"">
      <h1>&#9989; Xác thực tài khoản Relief Care</h1>
    </div>
    <div class=""body"">
      <p>Xin chào <strong>{displayName}</strong>,</p>
      <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>Relief Care</strong>. Vui lòng nhấn nút bên dưới để xác thực địa chỉ email và kích hoạt tài khoản của bạn.</p>
      <div class=""btn-wrap"">
        <a href=""{confirmLink}"" class=""btn"">Xác thực tài khoản</a>
      </div>
      <p>Hoặc dán đường link sau vào trình duyệt:</p>
      <p style=""word-break:break-all; font-size:13px; color:#1b5e20;"">{confirmLink}</p>
      <p class=""note"">&#9200; Link có hiệu lực trong <strong>24 giờ</strong>. Nếu bạn không đăng ký tài khoản này, hãy bỏ qua email — tài khoản sẽ tự động bị xóa sau 24 giờ.</p>
    </div>
    <div class=""footer"">© 2025 Relief Care System &nbsp;·&nbsp; support@reliefcare.app</div>
  </div>
</body>
</html>";
        }

        private static string BuildForgotPasswordEmailBody(string displayName, string resetLink)
        {
            return $@"<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Đặt lại mật khẩu</title>
  <style>
    body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f4f6f8; margin: 0; padding: 0; }}
    .wrapper {{ max-width: 560px; margin: 40px auto; background: #ffffff; border-radius: 12px;
                box-shadow: 0 4px 20px rgba(0,0,0,0.08); overflow: hidden; }}
    .header {{ background: linear-gradient(135deg, #1565c0, #42a5f5); padding: 32px 40px; text-align: center; }}
    .header h1 {{ color: #ffffff; margin: 0; font-size: 22px; letter-spacing: 0.5px; }}
    .body {{ padding: 32px 40px; color: #333; }}
    .body p {{ line-height: 1.7; margin: 0 0 16px; }}
    .btn-wrap {{ text-align: center; margin: 28px 0; }}
    .btn {{ display: inline-block; background: linear-gradient(135deg, #1565c0, #42a5f5);
            color: #ffffff !important; text-decoration: none; padding: 14px 36px;
            border-radius: 8px; font-weight: bold; font-size: 15px; letter-spacing: 0.3px; }}
    .note {{ font-size: 13px; color: #888; border-top: 1px solid #eee; padding-top: 16px; margin-top: 8px; }}
    .footer {{ background: #f4f6f8; text-align: center; padding: 16px; font-size: 12px; color: #aaa; }}
  </style>
</head>
<body>
  <div class=""wrapper"">
    <div class=""header"">
      <h1>&#128272; Đặt lại mật khẩu</h1>
    </div>
    <div class=""body"">
      <p>Xin chào <strong>{displayName}</strong>,</p>
      <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản <strong>Relief Care</strong> của bạn.</p>
      <div class=""btn-wrap"">
        <a href=""{resetLink}"" class=""btn"">Đặt lại mật khẩu</a>
      </div>
      <p>Hoặc dán đường link sau vào trình duyệt:</p>
      <p style=""word-break:break-all; font-size:13px; color:#1565c0;"">{resetLink}</p>
      <p class=""note"">&#9200; Link có hiệu lực trong <strong>1 giờ</strong>. Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này — tài khoản của bạn vẫn an toàn.</p>
    </div>
    <div class=""footer"">© 2025 Relief Care System &nbsp;·&nbsp; support@reliefcare.app</div>
  </div>
</body>
</html>";
        }

        private static IDictionary<string, string[]> ConvertErrors(IEnumerable<IdentityError> errors)
        {
            return errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Description).ToArray()
                );
        }
    }
}
