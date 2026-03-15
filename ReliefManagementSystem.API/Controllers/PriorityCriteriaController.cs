using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.PriorityCriteria.DTOs.Request;
using ReliefManagementSystem.Application.Features.PriorityCriteria.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/priority-criteria")]
    [ApiController]
    [Authorize(Roles = "Manager")] // Thường Manager mới có quyền CRUD danh mục
    public class PriorityCriteriaController : ControllerBase
    {
        private readonly IPriorityCriteriaService _priorityCriteriaService;

        public PriorityCriteriaController(IPriorityCriteriaService priorityCriteriaService)
        {
            _priorityCriteriaService = priorityCriteriaService;
        }

        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get all priority criteria")]
        [ProducesResponseType(typeof(List<PriorityCriteriaResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _priorityCriteriaService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get a priority criteria by ID")]
        [ProducesResponseType(typeof(PriorityCriteriaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _priorityCriteriaService.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new priority criteria")]
        [ProducesResponseType(typeof(PriorityCriteriaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreatePriorityCriteriaRequest request, CancellationToken cancellationToken)
        {
            var result = await _priorityCriteriaService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.PriorityCriteriaId }, result);
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Update an existing priority criteria")]
        [ProducesResponseType(typeof(PriorityCriteriaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePriorityCriteriaRequest request, CancellationToken cancellationToken)
        {
            var result = await _priorityCriteriaService.UpdateAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(Summary = "Delete (inactivate) a priority criteria")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _priorityCriteriaService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
    }
}
