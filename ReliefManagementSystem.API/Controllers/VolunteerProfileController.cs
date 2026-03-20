using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.VolunteerRequest.Request;
using ReliefManagementSystem.Application.Features.VolunteerRequest.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Application.Services;
using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteerProfileController : ControllerBase
    {
        private readonly IUserService _userService;

        public VolunteerProfileController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateVolunteerProfile([FromBody] CreateVolunteerRequest volunteerProfile)
        {
            var result = await _userService.CreateVolunteerProfileAsync(volunteerProfile);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest("Failed to create volunteer profile.");
        }

        [Authorize]
        [HttpGet("my-profile")]
        public async Task<ActionResult<VolunteerProfileResponse>> GetMyProfile(
            CancellationToken cancellationToken)
        {
            var result = await _userService
                .GetMyVolunteerProfileAsync(cancellationToken);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<List<VolunteerProfileResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _userService
                .GetAllVolunteerProfilesAsync(cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = "Moderator")]
        [HttpPut("{id:guid}/approve")]
        public async Task<ActionResult<VolunteerProfileResponse>> Approve(Guid id,CancellationToken cancellationToken)
        {
            var result = await _userService
                .ApproveVolunteerProfileAsync(id, cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = "Moderator")]
        [HttpPut("{id:guid}/reject")]
        public async Task<ActionResult<VolunteerProfileResponse>> Reject(Guid id,[FromBody] string reason,CancellationToken cancellationToken)
        {
            var result = await _userService
                .RejectVolunteerProfileAsync(id, reason, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Add new skills to current volunteer
        /// </summary>
        [HttpPost("skills")]
        public async Task<ActionResult<VolunteerProfileResponse>> AddSkills(
            [FromBody] AddVolunteerRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService
                .AddNewSkillVolunteer(request, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Remove skills from current volunteer
        /// </summary>
        [HttpDelete("skills")]
        public async Task<ActionResult<VolunteerProfileResponse>> RemoveSkills(
            [FromBody] RemoveVolunteerSkillRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _userService
                .RemoveSkillVolunteer(request, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Get all skills of current volunteer
        /// </summary>
        [HttpGet("skills")]
        public async Task<ActionResult<List<VolunteerSkillResponse>>> GetAllSkills(
            CancellationToken cancellationToken)
        {
            var result = await _userService
                .GetAllSkillsOfVolunteerAsync(cancellationToken);

            return Ok(result);
        }

    }
}
