using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Interface;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/funds")]
    [ApiController]
    public class FundController : ControllerBase
    {
        private readonly IFundService _fundService;

        public FundController(IFundService fundService)
        {
            _fundService = fundService;
        }

        [AllowAnonymous]
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var result = await _fundService.GetSummaryAsync(cancellationToken);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("contributions")]
        public async Task<IActionResult> GetContributions(CancellationToken cancellationToken)
        {
            var result = await _fundService.GetContributionsAsync(cancellationToken);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(CancellationToken cancellationToken)
        {
            var result = await _fundService.GetTransactionsAsync(cancellationToken);
            return Ok(result);
        }
    }
}
