using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Skill.Dtos;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Application.Services;
using ReliefManagementSystem.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly ISkillService _skillService;

        public SkillController(ISkillService skillService)
        {
            _skillService = skillService;
        }

        [HttpGet]
        [SwaggerOperation(OperationId = "GetAllSkills", Description = "Lấy danh sách kỹ năng có phân trang và tìm kiếm theo Code, Name, Description")]
        public async Task<ActionResult<Pagination<SkillResponse>>> GetAllSkills(
           [FromQuery] SearchSkillRequest request,
           CancellationToken cancellationToken)
        {
            var skills = await _skillService.GetAllSkillsAsync(request, cancellationToken);
            return Ok(skills);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<SkillResponse>> GetSkillById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var skill = await _skillService.GetSkillByIdAsync(id, cancellationToken);

            if (skill == null)
                return NotFound();

            return Ok(skill);
        }

        [HttpPost]
        public async Task<ActionResult<SkillResponse>> CreateSkill(
            [FromBody] CreateSkillRequest request,
            CancellationToken cancellationToken)
        {
            var skill = await _skillService.CreateSkillAsync(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetSkillById),
                new { id = skill.SkillId },
                skill);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateSkill(Guid id,[FromBody] UpdateSkillRequest request,CancellationToken cancellationToken)
        {
            await _skillService.UpdateSkillAsync(id, request, cancellationToken);
            return NoContent();
        }


        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteSkill(
            Guid id,
            CancellationToken cancellationToken)
        {
            await _skillService.DeleteSkillAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
