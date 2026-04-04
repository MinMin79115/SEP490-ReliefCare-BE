using ReliefManagementSystem.Application.Common.Exceptions.PriorityCriteriaExceptions;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.PriorityCriteria.DTOs.Request;
using ReliefManagementSystem.Application.Features.PriorityCriteria.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Services
{
    public class PriorityCriteriaService : IPriorityCriteriaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PriorityCriteriaService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PriorityCriteriaResponse> CreateAsync(CreatePriorityCriteriaRequest request, CancellationToken cancellationToken)
        {
            var existingByCode = await _unitOfWork.PriorityCriterias.GetByCodeAsync(request.Code, cancellationToken);
            if (existingByCode != null)
            {
                throw new DuplicatePriorityCriteriaCodeException(request.Code);
            }

            var entity = new PriorityCriteria
            {
                PriorityCriteriaId = Guid.NewGuid(),
                Name = request.Name,
                Point = request.Point,
                DisasterType = request.DisasterType,
                Code = request.Code,
                Description = request.Description ?? string.Empty,
                Status = "Active"
            };

            await _unitOfWork.PriorityCriterias.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(entity);
        }

        public async Task<PriorityCriteriaResponse> UpdateAsync(Guid id, UpdatePriorityCriteriaRequest request, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.PriorityCriterias.GetByIdAsync(id);
            if (entity == null)
            {
                throw new PriorityCriteriaNotFoundException(id);
            }

            if (entity.Code != request.Code)
            {
                var existingByCode = await _unitOfWork.PriorityCriterias.GetByCodeAsync(request.Code, cancellationToken);
                if (existingByCode != null)
                {
                    throw new DuplicatePriorityCriteriaCodeException(request.Code);
                }
            }

            entity.Name = request.Name;
            entity.Point = request.Point;
            entity.DisasterType = request.DisasterType;
            entity.Code = request.Code;
            entity.Description = request.Description ?? string.Empty;
            entity.Status = request.Status;

            _unitOfWork.PriorityCriterias.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(entity);
        }

        public async Task<PriorityCriteriaResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.PriorityCriterias.GetByIdAsync(id);
            if (entity == null)
            {
                throw new PriorityCriteriaNotFoundException(id);
            }

            return MapToResponse(entity);
        }

        public async Task<Pagination<PriorityCriteriaResponse>> GetAllAsync(
            SearchPriorityCriteriaRequest request,
            CancellationToken cancellationToken)
        {
            var query = _unitOfWork.PriorityCriterias.GetQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                query = query.Where(x =>
                    x.Name.Contains(keyword) ||
                    x.Code.Contains(keyword) ||
                    x.Description.Contains(keyword));
            }

            query = query.OrderByDescending(x => x.Point)
                         .ThenBy(x => x.Name);

            var paged = await Pagination<PriorityCriteria>.ToPagedList(
                query,
                request.PageIndex,
                request.PageSize);

            var items = paged.Items!.Select(MapToResponse).ToList();

            return new Pagination<PriorityCriteriaResponse>(
                items,
                paged.TotalCount,
                paged.CurrentPage,
                paged.PageSize);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _unitOfWork.PriorityCriterias.GetByIdAsync(id);
            if (entity == null)
            {
                throw new PriorityCriteriaNotFoundException(id);
            }

            // Instead of hard delete, we set it to Inactive based on current DB patterns, or if DeleteAsync exists in repository we can hard delete.
            // Using Update to inactive is safer.
            entity.Status = "Inactive";
            _unitOfWork.PriorityCriterias.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private PriorityCriteriaResponse MapToResponse(PriorityCriteria entity)
        {
            return new PriorityCriteriaResponse
            {
                PriorityCriteriaId = entity.PriorityCriteriaId,
                Name = entity.Name,
                Point = entity.Point,
                DisasterType = entity.DisasterType,
                Code = entity.Code,
                Description = entity.Description,
                Status = entity.Status
            };
        }
    }
}
