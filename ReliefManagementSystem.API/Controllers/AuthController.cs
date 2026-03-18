using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.Auth.DTOs;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace ReliefManagementSystem.API.Controllers
{
    /// <summary>
    /// API xử lý xác thực người dùng: đăng ký, đăng nhập, OAuth
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [Tags("Authentication")]
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

        /// <summary>
        /// Đăng ký tài khoản mới
        /// </summary>
        /// <remarks>
        /// Tạo tài khoản người dùng mới với email, username và password.
        /// 
        /// Yêu cầu:
        /// - Email phải là định dạng email hợp lệ và chưa được sử dụng
        /// - Username phải là duy nhất trong hệ thống
        /// - Password phải có ít nhất 6 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt
        /// - PhoneNumber phải là số điện thoại hợp lệ
        /// 
        /// Sample request:
        /// 
        ///     POST /api/Auth/register
        ///     {
        ///         "email": "user@example.com",
        ///         "userName": "newuser",
        ///         "password": "SecurePass123!",
        ///         "phoneNumber": "0901234567",
        ///         "fullName": "Nguyen Van A"
        ///     }
        /// </remarks>
        /// <param name="request">Thông tin đăng ký</param>
        /// <param name="cancellationToken">Token hủy request</param>
        /// <returns>Token xác thực nếu đăng ký thành công</returns>
        /// <response code="200">Đăng ký thành công, trả về access token và refresh token</response>
        /// <response code="400">Dữ liệu không hợp lệ (email/username đã tồn tại, password yếu...)</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(
            RegisterRequest request,
            CancellationToken cancellationToken)
        {
            return Ok(await _authService.RegisterAsync(request, cancellationToken));
        }

        [HttpPost("verify-email-otp")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmailOtp(
            [FromBody] VerifyEmailOtpRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.VerifyEmailOtpAsync(request, cancellationToken);
            return Ok(new { message = "Email verified successfully." });
        }

        [HttpPost("resend-email-otp")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ResendEmailOtp(
            [FromBody] ResendEmailOtpRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.ResendEmailOtpAsync(request, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Đăng nhập bằng Email và Password
        /// </summary>
        /// <remarks>
        /// Xác thực người dùng bằng email và password.
        /// 
        /// Nếu đăng nhập thành công, hệ thống sẽ trả về:
        /// - **AccessToken**: JWT token để xác thực các request, có thời hạn ngắn (mặc định 15 phút)
        /// - **RefreshToken**: Token để lấy AccessToken mới khi hết hạn, có thời hạn dài hơn (mặc định 7 ngày)
        /// 
        /// Sample request:
        /// 
        ///     POST /api/Auth/login
        ///     {
        ///         "email": "user@example.com",
        ///         "password": "SecurePass123!"
        ///     }
        /// </remarks>
        /// <param name="request">Thông tin đăng nhập</param>
        /// <param name="cancellationToken">Token hủy request</param>
        /// <returns>Token xác thực nếu đăng nhập thành công</returns>
        /// <response code="200">Đăng nhập thành công</response>
        /// <response code="401">Email hoặc password không đúng</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            return Ok(await _authService.LoginAsync(request, cancellationToken));
        }

        /// <summary>
        /// Đăng nhập bằng Số điện thoại và Password
        /// </summary>
        /// <remarks>
        /// Xác thực người dùng bằng số điện thoại thay vì email.
        /// 
        /// Phương thức này hữu ích cho người dùng ưa thích đăng nhập bằng số điện thoại
        /// hoặc không nhớ email đăng ký.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/Auth/phone-login
        ///     {
        ///         "phoneNumber": "0901234567",
        ///         "password": "SecurePass123!"
        ///     }
        /// </remarks>
        /// <param name="request">Thông tin đăng nhập bằng số điện thoại</param>
        /// <param name="cancellationToken">Token hủy request</param>
        /// <returns>Token xác thực nếu đăng nhập thành công</returns>
        /// <response code="200">Đăng nhập thành công</response>
        /// <response code="401">Số điện thoại hoặc password không đúng</response>
        [HttpPost("phone-login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginPhone(
            LoginPhoneRequest request,
            CancellationToken cancellationToken)
        {
            return Ok(await _authService.LoginPhoneAsync(request, cancellationToken));
        }

        /// <summary>
        /// Bắt đầu đăng nhập bằng Google OAuth
        /// </summary>
        /// <remarks>
        /// Chuyển hướng người dùng đến trang đăng nhập Google.
        /// 
        /// **Flow OAuth:**
        /// 1. Gọi endpoint này → Chuyển hướng đến Google
        /// 2. Người dùng đăng nhập Google → Google chuyển hướng về `/api/Auth/google-callback`
        /// 3. Hệ thống xử lý và trả về token
        /// 
        /// **Lưu ý:** Endpoint này phải được gọi từ browser, không phải từ API client như Postman.
        /// </remarks>
        /// <returns>Chuyển hướng đến trang đăng nhập Google</returns>
        /// <response code="302">Chuyển hướng đến Google OAuth</response>
        [HttpGet("google-login")]
        [ProducesResponseType(StatusCodes.Status302Found)]
        public IActionResult GoogleLogin()
        {
            var props = _signInManager
                .ConfigureExternalAuthenticationProperties(
                    GoogleDefaults.AuthenticationScheme,
                    Url.Action(nameof(GoogleCallback))
                );

            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Callback xử lý sau khi đăng nhập Google thành công
        /// </summary>
        /// <remarks>
        /// Endpoint này được Google gọi sau khi người dùng đăng nhập thành công.
        /// 
        /// **Không gọi trực tiếp endpoint này!** 
        /// Đây là callback URL được Google OAuth sử dụng tự động.
        /// 
        /// Sau khi xử lý:
        /// - Nếu user chưa tồn tại → Tạo tài khoản mới
        /// - Nếu user đã tồn tại → Đăng nhập
        /// - Trả về token xác thực
        /// </remarks>
        /// <param name="cancellationToken">Token hủy request</param>
        /// <returns>Token xác thực nếu thành công</returns>
        /// <response code="200">Đăng nhập Google thành công</response>
        /// <response code="401">Đăng nhập Google thất bại</response>
        [HttpGet("google-callback")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GoogleCallback(
            CancellationToken cancellationToken)
        {
            var user = await _authService
                .LoginGoogleAsync(cancellationToken);

            if (user == null)
                return Unauthorized("Google login failed");

            return Ok(new { user });
        }

        [Authorize]
        [SwaggerOperation(
            Summary = "Change password",
            Description = "Change password for the currently authenticated user."
        )]
        [SwaggerResponse(204, "Password changed successfully")]
        [SwaggerResponse(400, "Validation error")]
        [SwaggerResponse(401, "Unauthorized")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.ChangePasswordAsync(
                request,
                cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Bước 1: Gửi OTP quên mật khẩu qua mobile/email
        /// </summary>
        [HttpPost("forgot-password/send-otp")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendForgotPasswordOtp(
            [FromBody] SendForgotPasswordOtpRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.SendForgotPasswordOtpAsync(request, cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Bước 2: Xác thực OTP quên mật khẩu và nhận reset token
        /// </summary>
        [HttpPost("forgot-password/verify-otp")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyForgotPasswordOtp(
            [FromBody] VerifyForgotPasswordOtpRequest request,
            CancellationToken cancellationToken)
        {
            var response = await _authService.VerifyForgotPasswordOtpAsync(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Bước 3: Đặt lại mật khẩu bằng reset token sau khi xác thực OTP
        /// </summary>
        [HttpPost("forgot-password/reset")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPasswordByToken(
            [FromBody] ResetPasswordByTokenRequest request,
            CancellationToken cancellationToken)
        {
            await _authService.ResetPasswordByTokenAsync(request, cancellationToken);
            return NoContent();
        }
    }
}

