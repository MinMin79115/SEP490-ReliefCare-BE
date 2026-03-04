using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Request;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.API.Controllers
{
    /// <summary>
    /// Manages relief stations and their team assignments.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReliefStationController : ControllerBase
    {
        private readonly IReliefStationService _stationService;

        public ReliefStationController(IReliefStationService stationService)
        {
            _stationService = stationService;
        }


    }
}
