using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.Donation.DTOs.Request;
using ReliefManagementSystem.Application.Interface;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/donations")]
    [ApiController]
    public class DonationController : ControllerBase
    {
        private readonly IDonationService _donationService;

        public DonationController(IDonationService donationService)
        {
            _donationService = donationService;
        }

        [HttpPost("checkout")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateCheckout(
            [FromBody] CreateDonationCheckoutRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _donationService.CreateCheckoutAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}/status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStatus(Guid id, CancellationToken cancellationToken)
        {
            var result = await _donationService.GetStatusAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("payment-return")]
        [AllowAnonymous]
        public IActionResult PaymentReturn(
            [FromQuery] string? code,
            [FromQuery] string? id,
            [FromQuery] bool? cancel,
            [FromQuery] string? status,
            [FromQuery] long? orderCode)
        {
            return Ok(new
            {
                message = "Return URL received",
                code,
                id,
                cancel,
                status,
                orderCode
            });
        }

        [HttpGet("payment-cancel")]
        [AllowAnonymous]
        public IActionResult PaymentCancel(
            [FromQuery] string? code,
            [FromQuery] string? id,
            [FromQuery] bool? cancel,
            [FromQuery] string? status,
            [FromQuery] long? orderCode)
        {
            return Ok(new
            {
                message = "Cancel URL received",
                code,
                id,
                cancel,
                status,
                orderCode
            });
        }

        [HttpPost("webhook/payos")]
        [AllowAnonymous]
        public async Task<IActionResult> PayOsWebhook([FromBody] PayOsWebhookRequest request, CancellationToken cancellationToken)
        {
            await _donationService.HandlePayOsWebhookAsync(request, cancellationToken);
            return Ok(new { message = "Webhook processed" });
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDonations([FromQuery] AdminDonationQueryRequest request, CancellationToken cancellationToken)
        {
            var result = await _donationService.GetAdminDonationsAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("admin/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDonationDetail(Guid id, CancellationToken cancellationToken)
        {
            var result = await _donationService.GetAdminDonationDetailAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPost("admin/{id:guid}/reconcile")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reconcile(Guid id, CancellationToken cancellationToken)
        {
            var result = await _donationService.ReconcileAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPost("admin/{id:guid}/cancel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CancelPending(Guid id, [FromQuery] string? reason, CancellationToken cancellationToken)
        {
            var result = await _donationService.CancelPendingAsync(id, reason, cancellationToken);
            return Ok(result);
        }

        [HttpGet("admin/stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStats([FromQuery] AdminDonationQueryRequest request, CancellationToken cancellationToken)
        {
            var result = await _donationService.GetStatsAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("admin/export")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportCsv([FromQuery] AdminDonationQueryRequest request, CancellationToken cancellationToken)
        {
            var csv = await _donationService.ExportCsvAsync(request, cancellationToken);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"donations-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }
    }
}
