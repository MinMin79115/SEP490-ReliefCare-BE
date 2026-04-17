using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Interface;
using Swashbuckle.AspNetCore.Annotations;

namespace ReliefManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        [SwaggerOperation(OperationId = "GetMyNotifications", Summary = "Lấy danh sách notification của user hiện tại")]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _notificationService.GetMyNotificationsAsync(pageNumber, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpGet("unread-count")]
        [SwaggerOperation(OperationId = "GetUnreadNotificationCount", Summary = "Lấy số notification chưa đọc")]
        public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default)
        {
            var result = await _notificationService.GetUnreadCountAsync(cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{notificationId:guid}/read")]
        [SwaggerOperation(OperationId = "MarkNotificationAsRead", Summary = "Đánh dấu 1 notification là đã đọc")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken = default)
        {
            var result = await _notificationService.MarkAsReadAsync(notificationId, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("read-all")]
        [SwaggerOperation(OperationId = "MarkAllNotificationsAsRead", Summary = "Đánh dấu tất cả notification là đã đọc")]
        public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
        {
            var result = await _notificationService.MarkAllAsReadAsync(cancellationToken);
            return Ok(result);
        }
    }
}
