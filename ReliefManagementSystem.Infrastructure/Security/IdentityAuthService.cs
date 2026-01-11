using Microsoft.AspNetCore.Identity;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.Auth.DTOs;
using ReliefManagementSystem.Application.Features.Auth.Interface;
using ReliefManagementSystem.Application.Services;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                throw new Exception(string.Join("; ",
                    result.Errors.Select(e => e.Description)));

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
                throw new UnauthorizedAccessException("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);

            var check = await _signInManager
                .CheckPasswordSignInAsync(user, password, false);

            if (!check.Succeeded)
                throw new UnauthorizedAccessException("Invalid credentials");

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
    }
}
