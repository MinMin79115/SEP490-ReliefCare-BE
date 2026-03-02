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

        // ═══════════════════════════════════════════════════
        //  ReliefStation CRUD
        // ═══════════════════════════════════════════════════

        /// <summary>Creates a new relief station.</summary>
        /// <response code="200">Station created.</response>
        /// <response code="400">Name already exists.</response>
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateReliefStationRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _stationService.CreateAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Gets all active relief stations.</summary>
        /// <response code="200">List of stations.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _stationService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        /// <summary>Gets all relief stations filtered by status.</summary>
        /// <response code="200">Filtered list of stations.</response>
        [HttpGet("by-status")]
        public async Task<IActionResult> GetByStatus(
            [FromQuery] ReliefStationStatus status,
            CancellationToken cancellationToken)
        {
            var result = await _stationService.GetByStatusAsync(status, cancellationToken);
            return Ok(result);
        }

        /// <summary>Gets a single relief station with full details including teams.</summary>
        /// <response code="200">Station detail.</response>
        /// <response code="404">Station not found.</response>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _stationService.GetByIdAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>Updates a relief station.</summary>
        /// <response code="200">Station updated.</response>
        /// <response code="400">Name conflict.</response>
        /// <response code="404">Station not found.</response>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateReliefStationRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _stationService.UpdateAsync(id, request, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Closes (soft-deletes) a relief station.</summary>
        /// <response code="204">Station closed.</response>
        /// <response code="404">Station not found.</response>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _stationService.DeleteAsync(id, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ═══════════════════════════════════════════════════
        //  Team Assignment (nested under station)
        // ═══════════════════════════════════════════════════

        /// <summary>Gets all team assignments for a station.</summary>
        /// <response code="200">List of team assignments.</response>
        /// <response code="404">Station not found.</response>
        [HttpGet("{id:guid}/teams")]
        public async Task<IActionResult> GetTeams(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _stationService.GetTeamsAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>Assigns a team to a relief station.</summary>
        /// <response code="200">Team assigned.</response>
        /// <response code="400">Team already assigned.</response>
        /// <response code="404">Station or team not found.</response>
        [HttpPost("{id:guid}/teams")]
        public async Task<IActionResult> AssignTeam(
            Guid id,
            [FromBody] AssignTeamRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _stationService.AssignTeamAsync(id, request, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Updates the assignment status of a team at a station.</summary>
        /// <response code="200">Assignment updated.</response>
        /// <response code="404">Assignment not found.</response>
        [HttpPut("teams/{assignmentId:guid}")]
        public async Task<IActionResult> UpdateTeamAssignment(
            Guid assignmentId,
            [FromBody] UpdateTeamAssignmentRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _stationService.UpdateTeamAssignmentAsync(assignmentId, request, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>Removes a team from a relief station (hard delete of assignment record).</summary>
        /// <response code="204">Team removed.</response>
        /// <response code="404">Assignment not found.</response>
        [HttpDelete("teams/{assignmentId:guid}")]
        public async Task<IActionResult> RemoveTeam(Guid assignmentId, CancellationToken cancellationToken)
        {
            try
            {
                await _stationService.RemoveTeamAsync(assignmentId, cancellationToken);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
