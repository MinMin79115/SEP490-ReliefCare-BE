using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.Auth.DTOs;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(
            IAuthService authService,
            SignInManager<ApplicationUser> signInManager)
        {
            _authService = authService;
            _signInManager = signInManager;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterRequest request,
            CancellationToken cancellationToken)
        {
            return Ok(await _authService.RegisterAsync(request, cancellationToken));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            return Ok(await _authService.LoginAsync(request, cancellationToken));
        }

        [HttpPost("phone-login")]
        public async Task<IActionResult> LoginPhone(
            LoginPhoneRequest request,
            CancellationToken cancellationToken)
        {
            return Ok(await _authService.LoginPhoneAsync(request, cancellationToken));
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var props = _signInManager
                .ConfigureExternalAuthenticationProperties(
                    GoogleDefaults.AuthenticationScheme,
                    Url.Action(nameof(GoogleCallback))
                );

            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }


        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(
    CancellationToken cancellationToken)
        {
            var user = await _authService
                .LoginGoogleAsync(cancellationToken);

            if (user == null)
                return Unauthorized("Google login failed");

            return Ok(new { user });
        }

    }
}
