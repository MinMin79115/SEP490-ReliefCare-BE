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
        [SwaggerOperation(OperationId = "GetAllProfiles", Description = "Admin lấy danh sách tất cả users có phân trang, tìm kiếm theo DisplayName/Email/PhoneNumber, lọc theo Role và trạng thái bị ban")]
        public async Task<IActionResult> GetAllProfiles(
            [FromQuery] GetAllUsersRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService.GetAllProfilesAsync(request, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách moderator có phân trang (dành cho Admin), kèm trạng thái có đang quản lý trạm hay không.
        /// </summary>
        [HttpGet("moderators")]
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(OperationId = "GetModerators", Description = "Admin lấy danh sách moderator có phân trang, hỗ trợ tìm kiếm và lọc bị ban/không bị ban; kèm trường IsManagingStation")]
        public async Task<IActionResult> GetModerators(
            [FromQuery] GetModeratorsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService.GetModeratorsAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("moderators")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(OperationId = "CreateModeratorAccount", Description = "Admin tạo account Moderator và moderator profile")]
        public async Task<IActionResult> CreateModerator(
            [FromBody] CreateModeratorAccountRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService.CreateModeratorAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("moderators/{userId:guid}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(OperationId = "GetModeratorById", Description = "Admin lấy chi tiết account Moderator")]
        public async Task<IActionResult> GetModeratorById(Guid userId, CancellationToken cancellationToken)
        {
            var result = await _userService.GetModeratorByIdAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpPut("moderators/{userId:guid}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(OperationId = "UpdateModeratorAccount", Description = "Admin cập nhật account Moderator và moderator profile")]
        public async Task<IActionResult> UpdateModerator(
            Guid userId,
            [FromBody] UpdateModeratorAccountRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateModeratorAsync(userId, request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("moderators/{userId:guid}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(OperationId = "SoftDeleteModeratorAccount", Description = "Admin soft delete account Moderator bằng cách khóa tài khoản và đánh dấu profile dismissed")]
        public async Task<IActionResult> SoftDeleteModerator(
            Guid userId,
            [FromBody] SoftDeletePrivilegedAccountRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService.SoftDeleteModeratorAsync(userId, request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("managers")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(OperationId = "GetManagers", Description = "Admin lấy danh sách manager có phân trang")]
        public async Task<IActionResult> GetManagers(
            [FromQuery] GetManagersRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService.GetManagersAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("managers")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(OperationId = "CreateManagerAccount", Description = "Admin tạo account Manager và manager profile")]
        public async Task<IActionResult> CreateManager(
            [FromBody] CreateManagerAccountRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService.CreateManagerAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("managers/{userId:guid}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(OperationId = "GetManagerById", Description = "Admin lấy chi tiết account Manager")]
        public async Task<IActionResult> GetManagerById(Guid userId, CancellationToken cancellationToken)
        {
            var result = await _userService.GetManagerByIdAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpPut("managers/{userId:guid}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(OperationId = "UpdateManagerAccount", Description = "Admin cập nhật account Manager và manager profile")]
        public async Task<IActionResult> UpdateManager(
            Guid userId,
            [FromBody] UpdateManagerAccountRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateManagerAsync(userId, request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("managers/{userId:guid}")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(OperationId = "SoftDeleteManagerAccount", Description = "Admin soft delete account Manager bằng cách khóa tài khoản")]
        public async Task<IActionResult> SoftDeleteManager(
            Guid userId,
            [FromBody] SoftDeletePrivilegedAccountRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService.SoftDeleteManagerAsync(userId, request, cancellationToken);
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

        [HttpGet("my-volunteer-profile")]
        [Authorize]
        [SwaggerOperation(OperationId = "GetMyVolunteerProfile", Description = "Lấy hồ sơ volunteer của user đang đăng nhập")]
        public async Task<IActionResult> GetMyVolunteerProfile(CancellationToken cancellationToken)
        {
            var result = await _userService.GetMyVolunteerProfileAsync(cancellationToken);
            return Ok(result);
        }

        [HttpPut("{userId:guid}/ban")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(OperationId = "BanUser", Description = "Admin khóa tài khoản user và lưu lý do bị ban")]
        public async Task<IActionResult> BanUser(
            Guid userId,
            [FromBody] BanUserRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService.BanUserAsync(userId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{userId:guid}/unban")]
        [Authorize(Roles = "Admin")]
        [SwaggerOperation(OperationId = "UnbanUser", Description = "Admin mở khóa tài khoản user")]
        public async Task<IActionResult> UnbanUser(
            Guid userId,
            [FromBody] UnbanUserRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService.UnbanUserAsync(userId, request, cancellationToken);
            return Ok(result);
        }
    }
}
