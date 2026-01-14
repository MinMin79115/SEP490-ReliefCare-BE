using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.VolunteerRequest.Request;
using ReliefManagementSystem.Application.Features.VolunteerRequest.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolunteerProfileController : ControllerBase
    {
        private readonly IUserService userService;

        public VolunteerProfileController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateVolunteerProfile([FromBody] CreateVolunteerRequest volunteerProfile)
        {
            var result = await userService.CreateVolunteerProfileAsync(volunteerProfile);
            if (result != null)
            {
                return Ok(result);
            }
            return BadRequest("Failed to create volunteer profile.");
        }
    }
}
