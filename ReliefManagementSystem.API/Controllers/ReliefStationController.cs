using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.ReliefStation.Dtos;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Request;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Enum;
using Swashbuckle.AspNetCore.Annotations;

namespace ReliefManagementSystem.API.Controllers
{
    /// <summary>
    /// Quản lý trạm cứu trợ (ReliefStation) và phân công Team.
    /// </summary>
    [Route("api/relief-stations")]
    [ApiController]
    [Authorize]
    public class ReliefStationController : ControllerBase
    {
        private readonly IReliefStationService _stationService;

        public ReliefStationController(IReliefStationService stationService)
        {
            _stationService = stationService;
        }
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(OperationId = "CreateReliefStation", Description = "Manager tạo trạm mới cho tỉnh")]
        [HttpPost("provincial")]
        public async Task<IActionResult> CreateProvincialStation(
            CreateProvincialReliefStationRequest request, CancellationToken cancellationToken)
        {
            var result = await _stationService.CreateProvincialReliefStationAsync(request,cancellationToken);
            return Ok(result);
        }
    }
}
