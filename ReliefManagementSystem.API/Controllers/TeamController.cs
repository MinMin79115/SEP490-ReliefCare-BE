using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.Team.DTOs.Request;
using ReliefManagementSystem.Application.Features.Team.Interface;
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
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request, CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _teamService.CreateTeamAsync(request, moderatorId, cancellationToken);
            return Ok(result);
        }

        // GET /api/team/5
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetTeamById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _teamService.GetTeamByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        // PUT /api/team/5
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> UpdateTeam(Guid id, [FromBody] UpdateTeamRequest request, CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _teamService.UpdateTeamAsync(id, request, moderatorId, cancellationToken);
            return Ok(result);
        }

        // DELETE /api/team/5
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> DeleteTeam(Guid id, CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            await _teamService.DeleteTeamAsync(id, moderatorId, cancellationToken);
            return NoContent();
        }

        // GET /api/team
        [HttpGet]
        public async Task<IActionResult> GetAllTeams(CancellationToken cancellationToken)
        {
            var result = await _teamService.GetAllTeamsAsync(cancellationToken);
            return Ok(result);
        }

        // GET /api/team/search?name=abc&status=1&pageIndex=1&pageSize=10
        [HttpGet("search")]
        public async Task<IActionResult> SearchTeams([FromQuery] SearchTeamRequest request, CancellationToken cancellationToken)
        {
            var result = await _teamService.SearchTeamsAsync(request, cancellationToken);
            return Ok(result);
        }

        // GET /api/team/my-teams
        [HttpGet("my-teams")]
        [Authorize(Roles = "Moderator")]
        public async Task<IActionResult> GetMyTeams(CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _teamService.GetMyTeamsAsync(moderatorId, cancellationToken);
            return Ok(result);
        }

        // GET /api/team/5/members
        [HttpGet("{id:guid}/members")]
        public async Task<IActionResult> GetTeamMembers(Guid id, CancellationToken cancellationToken)
        {
            var result = await _teamService.GetTeamMembersAsync(id, cancellationToken);
            return Ok(result);
        }

        // DELETE /api/team/5/members/{userId}
        [HttpDelete("{id:int}/members/{userId:guid}")]
        [Authorize(Roles = "Moderator")]
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
