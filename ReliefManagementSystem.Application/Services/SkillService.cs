using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Skill.Dtos;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Services
{
    public class SkillService : ISkillService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SkillService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Pagination<SkillResponse>> GetAllSkillsAsync(
            SearchSkillRequest request,
            CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Skills.GetQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                query = query.Where(skill =>
                    (skill.Code ?? string.Empty).Contains(keyword) ||
                    (skill.Name ?? string.Empty).Contains(keyword) ||
                    (skill.Description ?? string.Empty).Contains(keyword));
            }

            query = query.OrderBy(skill => skill.Name);

            var pagedSkills = await Pagination<Skill>.ToPagedList(query, request.PageIndex, request.PageSize);

            var items = pagedSkills.Items!
                .Select(skill => new SkillResponse
                {
                    SkillId = skill.SkillId,
                    Code = skill.Code,
                    Name = skill.Name,
                    Description = skill.Description
                })
                .ToList();

            return new Pagination<SkillResponse>(items, pagedSkills.TotalCount, pagedSkills.CurrentPage, pagedSkills.PageSize);
        }


        public async Task<SkillResponse?> GetSkillByIdAsync(
    Guid skillId,
    CancellationToken cancellationToken)
        {
            var skill = await _unitOfWork.Skills.GetByIdAsync(skillId);

            if (skill == null)
                return null;

            return new SkillResponse
            {
                SkillId = skill.SkillId,
                Code = skill.Code,
                Name = skill.Name,
                Description = skill.Description
            };
        }


        public async Task<SkillResponse> CreateSkillAsync(CreateSkillRequest createSkillRequest,CancellationToken cancellationToken)
        {
            var skill = new Skill
            {
                Code = createSkillRequest.Code,
                Name = createSkillRequest.Name,
                Description = createSkillRequest.Description
            };

            await _unitOfWork.Skills.AddAsync(skill);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SkillResponse
            {
                SkillId = skill.SkillId,
                Code = skill.Code,
                Name = skill.Name,
                Description = skill.Description
            };
        }


        public async Task UpdateSkillAsync(
    Guid skillId,
    UpdateSkillRequest request,
    CancellationToken cancellationToken)
        {
            var skill = await _unitOfWork.Skills.GetByIdAsync(skillId);

            if (skill == null)
                throw new KeyNotFoundException("Skill not found.");

            skill.Name = request.Name;
            skill.Description = request.Description;

            _unitOfWork.Skills.UpdateAsync(skill);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }


        public async Task DeleteSkillAsync(
            Guid skillId,
            CancellationToken cancellationToken)
        {
            var skill = await _unitOfWork.Skills
                .GetByIdAsync(skillId);

            if (skill == null)
                throw new KeyNotFoundException("Skill not found.");

            _unitOfWork.Skills.DeleteAsync(skill);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

    }
}
