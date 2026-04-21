using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests;
using ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Responses;
using ReliefManagementSystem.Application.Interface;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/campaigns")]
    [ApiController]
    [Authorize]
    public class CampaignTaskController : ControllerBase
    {
        private readonly ICampaignTaskService _campaignTaskService;

        public CampaignTaskController(ICampaignTaskService campaignTaskService)
        {
            _campaignTaskService = campaignTaskService;
        }

        [HttpPost("{campaignId:guid}/tasks")]
        public async Task<IActionResult> Create(
            Guid campaignId,
            [FromBody] CreateCampaignTaskRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.CreateAsync(campaignId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { campaignTaskId = result.CampaignTaskId }, result);
        }

        [HttpGet("{campaignId:guid}/tasks")]
        public async Task<ActionResult<Pagination<CampaignTaskResponse>>> GetPaged(
            Guid campaignId,
            [FromQuery] CampaignTaskListQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.GetPagedAsync(campaignId, request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{campaignId:guid}/my-member-tasks")]
        public async Task<ActionResult<Pagination<MyMemberTaskResponse>>> GetMyMemberTasks(
            Guid campaignId,
            [FromQuery] MyMemberTaskQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.GetMyMemberTasksAsync(campaignId, request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("tasks/{campaignTaskId:guid}")]
        public async Task<ActionResult<CampaignTaskDetailResponse>> GetById(
            Guid campaignTaskId,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.GetByIdAsync(campaignTaskId, cancellationToken);
            return Ok(result);
        }

        [HttpPut("tasks/{campaignTaskId:guid}")]
        public async Task<ActionResult<CampaignTaskResponse>> Update(
            Guid campaignTaskId,
            [FromBody] UpdateCampaignTaskRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.UpdateAsync(campaignTaskId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("tasks/{campaignTaskId:guid}/status")]
        public async Task<ActionResult<CampaignTaskResponse>> ChangeStatus(
            Guid campaignTaskId,
            [FromBody] ChangeCampaignTaskStatusRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.ChangeStatusAsync(campaignTaskId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("tasks/{campaignTaskId:guid}/members")]
        public async Task<ActionResult<MemberTaskResponse>> AssignMember(
            Guid campaignTaskId,
            [FromBody] AssignMemberTaskRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.AssignMemberAsync(campaignTaskId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("tasks/{campaignTaskId:guid}/members/bulk")]
        public async Task<ActionResult<List<MemberTaskResponse>>> BulkAssignMembers(
            Guid campaignTaskId,
            [FromBody] BulkAssignMembersTaskRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.BulkAssignMembersAsync(campaignTaskId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("tasks/member-tasks/{memberTaskId:guid}/status")]
        public async Task<ActionResult<MemberTaskResponse>> ChangeMemberTaskStatus(
            Guid memberTaskId,
            [FromBody] ChangeMemberTaskStatusRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.ChangeMemberTaskStatusAsync(memberTaskId, request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("tasks/{campaignTaskId:guid}")]
        public async Task<IActionResult> Delete(
            Guid campaignTaskId,
            CancellationToken cancellationToken)
        {
            await _campaignTaskService.DeleteAsync(campaignTaskId, cancellationToken);
            return NoContent();
        }
    }
}
