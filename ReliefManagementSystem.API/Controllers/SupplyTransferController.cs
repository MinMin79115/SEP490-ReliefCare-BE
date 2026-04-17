using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.SupplyTransfer.DTOs.Request;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SupplyTransferController : ControllerBase
    {
        private readonly ISupplyTransferService _transferService;

        public SupplyTransferController(ISupplyTransferService transferService)
        {
            _transferService = transferService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSupplyTransferRequest request, CancellationToken cancellationToken)
            => Ok(await _transferService.CreateAsync(request, cancellationToken));

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
            => Ok(await _transferService.GetByIdAsync(id, cancellationToken));

        [HttpGet("by-status")]
        public async Task<IActionResult> GetByStatus([FromQuery] SupplyTransferStatus status, CancellationToken cancellationToken)
            => Ok(await _transferService.GetByStatusAsync(status, cancellationToken));

        [HttpGet("by-source-station/{stationId:guid}")]
        public async Task<IActionResult> GetBySourceStation(Guid stationId, CancellationToken cancellationToken)
            => Ok(await _transferService.GetBySourceStationAsync(stationId, cancellationToken));

        [HttpGet("by-destination-station/{stationId:guid}")]
        public async Task<IActionResult> GetByDestinationStation(Guid stationId, CancellationToken cancellationToken)
            => Ok(await _transferService.GetByDestinationStationAsync(stationId, cancellationToken));

        [HttpPatch("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveSupplyTransferRequest request, CancellationToken cancellationToken)
            => Ok(await _transferService.ApproveAsync(id, request, cancellationToken));

        [HttpPatch("{id:guid}/ship")]
        public async Task<IActionResult> Ship(Guid id, [FromBody] ShipSupplyTransferRequest request, CancellationToken cancellationToken)
            => Ok(await _transferService.ShipAsync(id, request, cancellationToken));

        [HttpPatch("{id:guid}/receive")]
        public async Task<IActionResult> Receive(Guid id, [FromBody] ReceiveSupplyTransferRequest request, CancellationToken cancellationToken)
            => Ok(await _transferService.ReceiveAsync(id, request, cancellationToken));

        [HttpPatch("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelSupplyTransferRequest request, CancellationToken cancellationToken)
            => Ok(await _transferService.CancelAsync(id, request, cancellationToken));

        [HttpPut("{id:guid}/evidence-urls")]
        public async Task<IActionResult> ReplaceEvidenceUrls(Guid id, [FromBody] ReplaceSupplyTransferEvidenceUrlsRequest request, CancellationToken cancellationToken)
            => Ok(await _transferService.ReplaceEvidenceUrlsAsync(id, request, cancellationToken));

        [HttpPost("{id:guid}/evidences")]
        public async Task<IActionResult> AppendEvidenceUrls(Guid id, [FromBody] AppendSupplyTransferEvidenceUrlsRequest request, CancellationToken cancellationToken)
            => Ok(await _transferService.AppendEvidenceUrlsAsync(id, request, cancellationToken));
    }
}
