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

        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<VolunteerProfileResponse>> GetByUserId(
    Guid userId,
    CancellationToken cancellationToken)
        {
            var result = await _userService
                .GetVolunteerProfileByUserIdAsync(userId, cancellationToken);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<List<VolunteerProfileResponse>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _userService
                .GetAllVolunteerProfilesAsync(cancellationToken);

            return Ok(result);
        }

        [HttpPut("{id:guid}/approve")]
        public async Task<ActionResult<VolunteerProfileResponse>> Approve(Guid id,CancellationToken cancellationToken)
        {
            var result = await _userService
                .ApproveVolunteerProfileAsync(id, cancellationToken);

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}/reject")]
        public async Task<ActionResult<VolunteerProfileResponse>> Reject(Guid id,[FromBody] string reason,CancellationToken cancellationToken)
        {
            var result = await _userService
                .RejectVolunteerProfileAsync(id, reason, cancellationToken);

            return Ok(result);
        }



    }
}
