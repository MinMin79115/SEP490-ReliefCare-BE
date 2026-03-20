using ReliefManagementSystem.Domain.Enum;
using System;

namespace ReliefManagementSystem.Application.Features.PriorityCriteria.DTOs.Response
{
    public class PriorityCriteriaResponse
    {
        public Guid PriorityCriteriaId { get; set; }
        public string Name { get; set; } = null!;
        public int Point { get; set; }
        public DisasterType DisasterType { get; set; }
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
    }
}
