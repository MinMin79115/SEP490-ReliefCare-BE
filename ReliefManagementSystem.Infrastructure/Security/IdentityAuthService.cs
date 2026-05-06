using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
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
using System.Security.Cryptography;
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
        private readonly IUnitOfWork _unitOfWork;

        public IdentityAuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ICurrentUserService currentUserService,
            IEmailService emailService,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _currentUserService = currentUserService;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApplicationUser> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            ApplicationUser? user = null;

            try
            {
                user = new ApplicationUser
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

                var addRoleResult = await _userManager.AddToRoleAsync(user, Role.User.ToString());
                if (!addRoleResult.Succeeded)
                {
                    var errors = ConvertErrors(addRoleResult.Errors);
                    throw new ValidationException(errors);
                }

                // Gửi OTP 6 số xác thực sau khi tạo tài khoản thành công
                await SendEmailOtpAsync(user, cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                return user;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                // Fallback an toàn: nếu user đã được tạo nhưng vì lý do nào đó transaction không bao trùm,
                // thì xóa user để đảm bảo không lưu tài khoản nửa chừng.
                if (user != null)
                {
                    var existing = await _userManager.FindByIdAsync(user.Id.ToString());
                    if (existing != null)
                    {
                        await _userManager.DeleteAsync(existing);
                    }
                }

                throw;
            }
        }

        public async Task SendEmailOtpAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            var otp = await CreateOtpAsync(user, OtpPurpose.EmailVerification, cancellationToken);

            var subject = "Mã xác thực tài khoản - Relief Care";
            var body = BuildEmailOtpBody(user.DisplayName ?? user.Email!, otp);
            await _emailService.SendEmailAsync(user.Email!, subject, body);
        }

        public async Task VerifyEmailOtpAsync(string email, string code, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email)
                ?? throw new InvalidCredentialsException();

            if (user.EmailConfirmed)
                return;

            var otp = await _unitOfWork.EmailOtps.GetLatestValidAsync(user.Id, OtpPurpose.EmailVerification, cancellationToken);
            if (otp == null)
                throw new ValidationException(new Dictionary<string, string[]> { { "Code", new[] { "OTP không tồn tại hoặc đã hết hạn." } } });

            if (!VerifyOtpHash(code, otp.CodeHash))
                throw new ValidationException(new Dictionary<string, string[]> { { "Code", new[] { "OTP không đúng." } } });

            user.EmailConfirmed = true;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = ConvertErrors(updateResult.Errors);
                throw new ValidationException(errors);
            }

            otp.ConsumedAt = DateTime.UtcNow;
            await _unitOfWork.EmailOtps.UpdateAsync(otp);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task ResendEmailOtpAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.EmailConfirmed)
                return;

            await SendEmailOtpAsync(user, cancellationToken);
        }

        public async Task SendForgotPasswordOtpAsync(string email, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);

            // Không leak thông tin: nếu email không tồn tại thì vẫn trả về thành công
            if (user == null)
                return;

            var otp = await CreateOtpAsync(user, OtpPurpose.PasswordReset, cancellationToken);
            var subject = "Mã OTP đặt lại mật khẩu - Relief Care";
            var body = BuildForgotPasswordOtpBody(user.DisplayName ?? email, otp);

            await _emailService.SendEmailAsync(email, subject, body);
        }

        public async Task<string> VerifyForgotPasswordOtpAsync(string email, string otpCode, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new InvalidCredentialsException();

            var otp = await _unitOfWork.EmailOtps.GetLatestValidAsync(user.Id, OtpPurpose.PasswordReset, cancellationToken);
            if (otp == null)
                throw new ValidationException(new Dictionary<string, string[]> { { "Code", new[] { "OTP không tồn tại hoặc đã hết hạn." } } });

            if (!VerifyOtpHash(otpCode, otp.CodeHash))
                throw new ValidationException(new Dictionary<string, string[]> { { "Code", new[] { "OTP không đúng." } } });

            otp.ConsumedAt = DateTime.UtcNow;
            await _unitOfWork.EmailOtps.UpdateAsync(otp);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var internalToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var tokenBytes = Encoding.UTF8.GetBytes(internalToken);
            return WebEncoders.Base64UrlEncode(tokenBytes);
        }

        public async Task ResetPasswordByTokenAsync(string email, string resetToken, string newPassword, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                throw new InvalidCredentialsException();

            var tokenBytes = WebEncoders.Base64UrlDecode(resetToken);
            var internalToken = Encoding.UTF8.GetString(tokenBytes);

            var result = await _userManager.ResetPasswordAsync(user, internalToken, newPassword);

            if (!result.Succeeded)
            {
                var errors = ConvertErrors(result.Errors);
                throw new ValidationException(errors);
            }

            await _userManager.UpdateSecurityStampAsync(user);
        }

        public async Task<ApplicationUser> ValidateByEmailAsync(
            string email,
            string password,
            CancellationToken cancellationToken)
        {
            var identifier = email?.Trim();
            var user = await _userManager.FindByEmailAsync(identifier!);

            if (user == null && !string.IsNullOrWhiteSpace(identifier))
            {
                user = await _userManager.FindByNameAsync(identifier);
            }

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

        private static string BuildEmailOtpBody(string displayName, string otp)
        {
            return $@"<!DOCTYPE html>
<html lang=""vi""><head><meta charset=""UTF-8"" /><meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
<title>Mã xác thực</title></head>
<body style=""font-family:Segoe UI,Arial,sans-serif;background:#f4f6f8;padding:24px;"">
  <div style=""max-width:520px;margin:auto;background:#fff;border-radius:12px;padding:24px;"">
    <h2 style=""margin-top:0;color:#1b5e20;"">Xác thực tài khoản Relief Care</h2>
    <p>Xin chào <strong>{displayName}</strong>,</p>
    <p>Mã xác thực 6 số của bạn là:</p>
    <div style=""font-size:32px;font-weight:700;letter-spacing:8px;color:#1b5e20;margin:16px 0;"">{otp}</div>
    <p>Mã có hiệu lực trong <strong>10 phút</strong>.</p>
    <p style=""color:#666;font-size:13px;"">Nếu bạn không thực hiện đăng ký, hãy bỏ qua email này.</p>
  </div>
</body></html>";
        }

        private static string BuildForgotPasswordOtpBody(string displayName, string otp)
        {
            return $@"<!DOCTYPE html>
<html lang=""vi""><head><meta charset=""UTF-8"" /><meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
<title>Mã OTP đặt lại mật khẩu</title></head>
<body style=""font-family:Segoe UI,Arial,sans-serif;background:#f4f6f8;padding:24px;"">
  <div style=""max-width:520px;margin:auto;background:#fff;border-radius:12px;padding:24px;"">
    <h2 style=""margin-top:0;color:#1565c0;"">Đặt lại mật khẩu Relief Care</h2>
    <p>Xin chào <strong>{displayName}</strong>,</p>
    <p>Mã OTP 6 số để đặt lại mật khẩu của bạn là:</p>
    <div style=""font-size:32px;font-weight:700;letter-spacing:8px;color:#1565c0;margin:16px 0;"">{otp}</div>
    <p>Mã có hiệu lực trong <strong>10 phút</strong>.</p>
    <p style=""color:#666;font-size:13px;"">Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.</p>
  </div>
</body></html>";
        }

        private async Task<string> CreateOtpAsync(ApplicationUser user, OtpPurpose purpose, CancellationToken cancellationToken)
        {
            await _unitOfWork.EmailOtps.InvalidateAllActiveAsync(user.Id, purpose, cancellationToken);

            var code = Random.Shared.Next(100000, 1000000).ToString();
            var now = DateTime.UtcNow;

            var otp = new EmailOtp
            {
                EmailOtpId = Guid.NewGuid(),
                UserId = user.Id,
                Purpose = purpose,
                CodeHash = HashOtp(code),
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(10)
            };

            await _unitOfWork.EmailOtps.AddAsync(otp);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return code;
        }

        private static string HashOtp(string code)
        {
            var bytes = Encoding.UTF8.GetBytes(code);
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }

        private static bool VerifyOtpHash(string providedCode, string storedHash)
        {
            var providedHash = HashOtp(providedCode);
            var providedHashBytes = Encoding.UTF8.GetBytes(providedHash);
            var storedHashBytes = Encoding.UTF8.GetBytes(storedHash);

            if (providedHashBytes.Length != storedHashBytes.Length)
                return false;

            return CryptographicOperations.FixedTimeEquals(providedHashBytes, storedHashBytes);
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
