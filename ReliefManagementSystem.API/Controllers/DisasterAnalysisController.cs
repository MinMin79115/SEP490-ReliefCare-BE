using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.DisasterAnalysis.DTOs.Request;
using ReliefManagementSystem.Application.Features.DisasterAnalysis.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using Swashbuckle.AspNetCore.Annotations;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisasterAnalysisController : ControllerBase
    {
        private readonly IDisasterAnalysisService _disasterAnalysisService;

        public DisasterAnalysisController(IDisasterAnalysisService disasterAnalysisService)
        {
            _disasterAnalysisService = disasterAnalysisService;
        }

        [HttpPost("analyze")]
        [AllowAnonymous]
        [SwaggerOperation(
            OperationId = "AnalyzeDisasterRisk",
            Summary = "Phân tích nguy cơ thiên tai bằng weather + AI",
            Description = "Lấy thời tiết hiện tại theo lat/lng, tự ước lượng các nguy cơ thiên tai liên quan đến thời tiết, sau đó gọi LLM để sinh phân tích và khuyến nghị tham khảo bằng tiếng Việt. Có thể truyền DisasterType nếu muốn phân tích tập trung vào một loại cụ thể.")]
        [ProducesResponseType(typeof(AnalyzeDisasterRiskResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Analyze(
            [FromBody] AnalyzeDisasterRiskRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _disasterAnalysisService.AnalyzeAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }
    }
}
