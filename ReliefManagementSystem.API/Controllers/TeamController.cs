using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.Team.DTOs.Request;
using ReliefManagementSystem.Application.Interface;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;

        public TeamController(ITeamService teamService)
        {
            _teamService = teamService;
        }

        // POST /api/team
        [HttpPost]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "CreateTeam", Description = "Moderator tạo team mới")]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request, CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _teamService.CreateTeamAsync(request, moderatorId, cancellationToken);
            return Ok(result);
        }

        // GET /api/team/{id}
        [HttpGet("{id:guid}")]
        [SwaggerOperation(OperationId = "GetTeamById", Description = "Lấy thông tin chi tiết team")]
        public async Task<IActionResult> GetTeamById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _teamService.GetTeamByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        // PUT /api/team/{id}
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "UpdateTeam", Description = "Moderator cập nhật thông tin team")]
        public async Task<IActionResult> UpdateTeam(Guid id, [FromBody] UpdateTeamRequest request, CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _teamService.UpdateTeamAsync(id, request, moderatorId, cancellationToken);
            return Ok(result);
        }

        // DELETE /api/team/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "DeleteTeam", Description = "Moderator xóa team")]
        public async Task<IActionResult> DeleteTeam(Guid id, CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            await _teamService.DeleteTeamAsync(id, moderatorId, cancellationToken);
            return NoContent();
        }

        // GET /api/team
        [HttpGet]
        [SwaggerOperation(OperationId = "GetAllTeams", Description = "Lấy danh sách tất cả teams có phân trang và tìm kiếm theo Name, Description, ContactPhone")]
        public async Task<IActionResult> GetAllTeams(
            [FromQuery] SearchTeamRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _teamService.GetAllTeamsAsync(request, cancellationToken);
            return Ok(result);
        }


        // GET /api/team/search
        [HttpGet("search")]
        [SwaggerOperation(OperationId = "SearchTeams", Description = "Tìm kiếm teams có phân trang theo Search (Name, Description, ContactPhone), Name, Status, ModeratorId")]
        public async Task<IActionResult> SearchTeams([FromQuery] SearchTeamRequest request, CancellationToken cancellationToken)
        {
            var result = await _teamService.SearchTeamsAsync(request, cancellationToken);
            return Ok(result);
        }

        // GET /api/team/in-station
        [HttpGet("in-station")]
        [SwaggerOperation(OperationId = "GetTeamsInStation", Description = "Lấy danh sách team trong trạm, hỗ trợ phân trang + tìm theo tên team/leader")]
        public async Task<IActionResult> GetTeamsInStation([FromQuery] GetTeamsInStationRequest request, CancellationToken cancellationToken)
        {
            var result = await _teamService.GetTeamsInStationAsync(request, cancellationToken);
            return Ok(result);
        }

       // GET /api/team/my-teams
        [HttpGet("my-teams")]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "GetMyTeams", Description = "Moderator lấy tất cả teams mình quản lý bao gồm thông tin members")]
        public async Task<IActionResult> GetMyTeams(CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _teamService.GetMyTeamsWithMembersAsync(moderatorId, cancellationToken);
            return Ok(result);
        }

        // GET /api/team/my-team (cho Volunteer)
        [HttpGet("my-team")]
        [Authorize(Roles = "Volunteer")]
        [SwaggerOperation(OperationId = "GetMyTeam", Description = "Volunteer lấy team mà mình đang tham gia")]
        public async Task<IActionResult> GetMyTeam(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _teamService.GetVolunteerTeamAsync(userId, cancellationToken);
            return Ok(result);
        }

        // GET /api/team/{id}/members
        [HttpGet("{id:guid}/members")]
        [SwaggerOperation(OperationId = "GetTeamMembers", Description = "Lấy danh sách members của team")]
        public async Task<IActionResult> GetTeamMembers(Guid id, CancellationToken cancellationToken)
        {
            var result = await _teamService.GetTeamMembersAsync(id, cancellationToken);
            return Ok(result);
        }

        // POST /api/team/{id}/members (Moderator add volunteer trực tiếp)
        [HttpPost("{id:guid}/members")]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "AddMemberDirectly", Description = "Moderator thêm volunteer vào team trực tiếp")]
        public async Task<IActionResult> AddMemberDirectly(
            Guid id, 
            [FromBody] AddMemberRequest request, 
            CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _teamService.AddMemberDirectlyAsync(id, request, moderatorId, cancellationToken);
            return Ok(result);
        }

        // POST /api/team/{id}/members/bulk (Moderator add nhiều volunteer trực tiếp)
        [HttpPost("{id:guid}/members/bulk")]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "AddMembersDirectly", Description = "Moderator thêm 1 hoặc nhiều volunteer vào team trong một request")]
        public async Task<IActionResult> AddMembersDirectly(
            Guid id,
            [FromBody] AddMembersRequest request,
            CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _teamService.AddMembersDirectlyAsync(id, request, moderatorId, cancellationToken);
            return Ok(result);
        }

        // PATCH /api/team/{id}/members/{userId}/promote-to-leader
        [HttpPatch("{id:guid}/members/{userId:guid}/promote-to-leader")]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "PromoteMemberToLeader", Description = "Moderator cập nhật role của member lên Leader")]
        public async Task<IActionResult> PromoteMemberToLeader(
            Guid id, 
            Guid userId, 
            CancellationToken cancellationToken)
        {
            var result = await _teamService.PromoteMemberToLeaderAsync(id, userId, cancellationToken);
            return Ok(result);
        }

        // DELETE /api/team/{id}/members/{userId}
        [HttpDelete("{id:guid}/members/{userId:guid}")]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "RemoveMember", Description = "Moderator xóa member khỏi team")]
        public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            await _teamService.RemoveMemberAsync(id, userId, moderatorId, cancellationToken);
            return NoContent();
        }

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        }
    }
}
