using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.User;
using ReliefManagementSystem.Application.Interface;
using Swashbuckle.AspNetCore.Annotations;
using ValidationException = ReliefManagementSystem.Application.Common.Exceptions.ValidationException;

namespace ReliefManagementSystem.API.Controllers
{
    /// <summary>
    /// API quản lý thông tin user profile.
    /// Frontend sử dụng các endpoint dưới đây để lấy và cập nhật thông tin user.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IValidator<UpdateUserProfileRequest> _validator;

        public UserController(IUserService userService, IValidator<UpdateUserProfileRequest> validator)
        {
            _userService = userService;
            _validator = validator;
        }

        /// <summary>
        /// Lấy thông tin profile của user đang đăng nhập.
        /// Frontend gọi API này sau khi user login thành công để hiển thị thông tin cá nhân.
        /// </summary>
        /// <returns>UserProfileResponse chứa thông tin user</returns>
        /// <response code="200">Trả về profile user thành công</response>
        /// <response code="401">User chưa đăng nhập</response>
        /// <response code="404">User không tồn tại (AUTH_USER_NOT_FOUND)</response>
        [HttpGet("profile")]
        [Authorize]
        [SwaggerOperation(OperationId = "GetProfile", Description = "Lấy thông tin profile của user đang đăng nhập")]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            var result = await _userService.GetProfileAsync(cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách tất cả users có phân trang (dành cho Admin).
        /// Frontend truyền pageIndex và pageSize qua query string.
        /// Ví dụ: GET /api/user/all?pageIndex=1&amp;pageSize=10
        /// </summary>
        /// <param name="request">Thông tin phân trang: pageIndex (mặc định 1), pageSize (mặc định 10)</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Pagination&lt;UserProfileResponse&gt; với danh sách users và thông tin phân trang</returns>
        /// <response code="200">Trả về danh sách users phân trang thành công</response>
        /// <response code="401">User chưa đăng nhập</response>
        /// <response code="403">User không có quyền Admin</response>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(OperationId = "GetAllProfiles", Description = "Admin lấy danh sách tất cả users có phân trang")]
        public async Task<IActionResult> GetAllProfiles(
            [FromQuery] GetAllUsersRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService.GetAllProfilesAsync(request, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin profile của user đang đăng nhập (partial update).
        /// Frontend gửi form-data, chỉ cần gửi các field muốn cập nhật.
        /// Các field không gửi sẽ giữ nguyên giá trị cũ.
        /// Avatar là file upload (IFormFile).
        /// </summary>
        /// <param name="request">Thông tin cập nhật: DisplayName, PhoneNumber, DateOfBirth, Gender, Avatar (file)</param>
        /// <param name="cancellationToken"></param>
        /// <returns>UserProfileResponse với thông tin đã cập nhật</returns>
        /// <response code="200">Cập nhật profile thành công</response>
        /// <response code="400">Validation error (VALIDATION_ERROR)</response>
        /// <response code="401">User chưa đăng nhập</response>
        /// <response code="404">User không tồn tại (AUTH_USER_NOT_FOUND)</response>
        [HttpPut("profile")]
        [Authorize]
        [SwaggerOperation(OperationId = "UpdateUserProfile", Description = "Cập nhật thông tin profile của user đang đăng nhập (partial update)")]
        public async Task<IActionResult> UpdateUserProfile(
            [FromForm] UpdateUserProfileRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                throw new ValidationException(errors);
            }

            var result = await _userService.UpdateUserProfileAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}
