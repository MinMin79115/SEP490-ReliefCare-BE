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
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> Create(
            Guid campaignId,
            [FromBody] CreateCampaignTaskRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.CreateAsync(campaignId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { campaignTaskId = result.CampaignTaskId }, result);
        }

        [HttpGet("{campaignId:guid}/tasks")]
        [Authorize(Roles = "Admin,Manager,Moderator,Volunteer")]
        public async Task<ActionResult<Pagination<CampaignTaskResponse>>> GetPaged(
            Guid campaignId,
            [FromQuery] CampaignTaskListQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.GetPagedAsync(campaignId, request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("admin/task-aggregate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<AdminCampaignTaskAggregateResponse>>> GetAdminTaskAggregate(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] Guid? teamId,
            [FromQuery] Guid? campaignId,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.GetAdminTaskAggregateAsync(from, to, teamId, campaignId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("admin/top-teams")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<AdminTopTeamResponse>>> GetAdminTopTeams(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] Guid? teamId,
            [FromQuery] Guid? campaignId,
            [FromQuery] int top = 4,
            CancellationToken cancellationToken = default)
        {
            var result = await _campaignTaskService.GetAdminTopTeamsAsync(from, to, teamId, campaignId, top, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{campaignId:guid}/member-tasks/me")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<Pagination<MyMemberTaskResponse>>> GetMyMemberTasks(
            Guid campaignId,
            [FromQuery] MyMemberTaskQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.GetMyMemberTasksAsync(campaignId, request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{campaignId:guid}/member-task-deliveries/me")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<List<MemberTaskDeliveryResponse>>> GetMyMemberTaskDeliveries(
            Guid campaignId,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.GetMyMemberTaskDeliveriesAsync(campaignId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("tasks/{campaignTaskId:guid}")]
        [Authorize(Roles = "Admin,Manager,Moderator,Volunteer")]
        public async Task<ActionResult<CampaignTaskDetailResponse>> GetById(
            Guid campaignTaskId,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.GetByIdAsync(campaignTaskId, cancellationToken);
            return Ok(result);
        }

        [HttpPut("tasks/{campaignTaskId:guid}")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<CampaignTaskResponse>> Update(
            Guid campaignTaskId,
            [FromBody] UpdateCampaignTaskRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.UpdateAsync(campaignTaskId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("tasks/{campaignTaskId:guid}/status")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<CampaignTaskResponse>> ChangeStatus(
            Guid campaignTaskId,
            [FromBody] ChangeCampaignTaskStatusRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.ChangeStatusAsync(campaignTaskId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("tasks/{campaignTaskId:guid}/members")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<MemberTaskResponse>> AssignMember(
            Guid campaignTaskId,
            [FromBody] AssignMemberTaskRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.AssignMemberAsync(campaignTaskId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("tasks/{campaignTaskId:guid}/members/bulk")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<List<MemberTaskResponse>>> BulkAssignMembers(
            Guid campaignTaskId,
            [FromBody] BulkAssignMembersTaskRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.BulkAssignMembersAsync(campaignTaskId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("tasks/{campaignTaskId:guid}/members/from-households")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<List<MemberTaskResponse>>> CreateMemberTasksFromHouseholds(
            Guid campaignTaskId,
            [FromBody] CreateMemberTaskFromHouseholdsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.CreateMemberTasksFromHouseholdsAsync(campaignTaskId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("tasks/{campaignTaskId:guid}/members/batch-from-deliveries")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<List<MemberTaskResponse>>> BulkAssignDeliveriesToMembers(
            Guid campaignTaskId,
            [FromBody] BulkAssignDeliveriesToMembersRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.BulkAssignDeliveriesToMembersAsync(campaignTaskId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("member-tasks/{memberTaskId:guid}/deliveries")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<List<MemberTaskDeliveryResponse>>> AssignDeliveriesToMemberTask(
            Guid memberTaskId,
            [FromBody] AssignMemberTaskDeliveriesRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.AssignDeliveriesToMemberTaskAsync(memberTaskId, request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("member-tasks/{memberTaskId:guid}/deliveries")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<List<MemberTaskDeliveryResponse>>> GetMemberTaskDeliveries(
            Guid memberTaskId,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.GetMemberTaskDeliveriesAsync(memberTaskId, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("member-tasks/{memberTaskId:guid}/status")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<MemberTaskResponse>> ChangeMemberTaskStatus(
            Guid memberTaskId,
            [FromBody] ChangeMemberTaskStatusRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.ChangeMemberTaskStatusAsync(memberTaskId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("member-task-deliveries/{memberTaskDeliveryId:guid}/status")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<MemberTaskDeliveryResponse>> ChangeMemberTaskDeliveryStatus(
            Guid memberTaskDeliveryId,
            [FromBody] ChangeMemberTaskDeliveryStatusRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.ChangeMemberTaskDeliveryStatusAsync(memberTaskDeliveryId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("member-task-deliveries/{memberTaskDeliveryId:guid}/complete-with-delivery")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<MemberTaskDeliveryResponse>> CompleteMemberTaskDeliveryWithDelivery(
            Guid memberTaskDeliveryId,
            [FromBody] CompleteMemberTaskDeliveryWithDeliveryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignTaskService.CompleteMemberTaskDeliveryWithDeliveryAsync(memberTaskDeliveryId, request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("tasks/{campaignTaskId:guid}")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> Delete(
            Guid campaignTaskId,
            CancellationToken cancellationToken)
        {
            await _campaignTaskService.DeleteAsync(campaignTaskId, cancellationToken);
            return NoContent();
        }
    }
}
