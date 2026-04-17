using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Interface;
using Swashbuckle.AspNetCore.Annotations;

namespace ReliefManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/realtime")]
    [Authorize]
    public class RealtimeController : ControllerBase
    {
        private readonly IRealtimeTokenService _realtimeTokenService;

        public RealtimeController(IRealtimeTokenService realtimeTokenService)
        {
            _realtimeTokenService = realtimeTokenService;
        }

        [HttpGet("token")]
        [SwaggerOperation(OperationId = "GetRealtimeToken", Summary = "Lấy Centrifugo realtime connection token cho user hiện tại")]
        public async Task<IActionResult> GetToken(CancellationToken cancellationToken = default)
        {
            var result = await _realtimeTokenService.GenerateForCurrentUserAsync(cancellationToken);
            return Ok(result);
        }
    }
}
