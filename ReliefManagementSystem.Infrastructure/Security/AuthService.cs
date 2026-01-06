using Microsoft.AspNetCore.Identity;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.Auth;
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

    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthResponse> RegisterAsync(
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
                throw new Exception(string.Join(
                    "; ", result.Errors.Select(e => e.Description)));
            }

            await _userManager.AddToRoleAsync(user, Role.User.ToString());

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
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            var checkPassword = await _signInManager.CheckPasswordSignInAsync(
                user, request.Password, false);

            if (!checkPassword.Succeeded)
                throw new UnauthorizedAccessException("Invalid credentials");

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
            var user = await _userManager.FindByNameAsync(request.PhoneNumber);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");
            var checkPassword = await _signInManager.CheckPasswordSignInAsync(
                user, request.Password, false);
            if (!checkPassword.Succeeded)
                throw new UnauthorizedAccessException("Invalid credentials");
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

    }
}
