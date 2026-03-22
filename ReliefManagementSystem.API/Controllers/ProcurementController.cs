using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.Procurement.Dtos.Requests;
using ReliefManagementSystem.Application.Interface;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/procurements")]
    [ApiController]
    [Authorize]
    public class ProcurementController : ControllerBase
    {
        private readonly IProcurementService _procurementService;

        public ProcurementController(IProcurementService procurementService)
        {
            _procurementService = procurementService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProcurementOrderRequest request, CancellationToken cancellationToken)
        {
            var result = await _procurementService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.ProcurementOrderId }, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _procurementService.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("by-campaign/{campaignId:guid}")]
        public async Task<IActionResult> GetByCampaign(Guid campaignId, CancellationToken cancellationToken)
        {
            var result = await _procurementService.GetByCampaignAsync(campaignId, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveProcurementOrderRequest request, CancellationToken cancellationToken)
        {
            var result = await _procurementService.ApproveAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/receive")]
        public async Task<IActionResult> Receive(Guid id, [FromBody] ReceiveProcurementOrderRequest request, CancellationToken cancellationToken)
        {
            var result = await _procurementService.ReceiveAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        {
            var result = await _procurementService.CancelAsync(id, cancellationToken);
            return Ok(result);
        }
    }
}
