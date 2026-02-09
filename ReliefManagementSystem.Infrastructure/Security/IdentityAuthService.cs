using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using ReliefManagementSystem.Application.Common.Exceptions.Auth;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.Auth.DTOs;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Application.Services;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Application.Common.Exceptions;

namespace ReliefManagementSystem.Infrastructure.Security
{

    public class IdentityAuthService : IIdentityAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IdentityAuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

            return user;
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

        //public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken)
        //{
        //    var user = await _userManager.FindByEmailAsync(email);

        //    if (user == null)
        //        return; // ❗ Không leak thông tin user tồn tại hay không

        //    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        //    var resetLink = $"https://your-frontend/reset-password?email={email}&token={Uri.EscapeDataString(token)}";

        //    // TODO: Send email
        //    await _emailService.SendAsync(email, "Reset Password", resetLink);
        //}

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
