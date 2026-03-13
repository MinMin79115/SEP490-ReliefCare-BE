using System;

namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Response
{
    /// <summary>DTO cho tiêu chí ưu tiên</summary>
    public class PriorityCriteriaDto
    {
        public Guid PriorityCriteriaId { get; set; }

        public string Name { get; set; } = null!;

        public int Point { get; set; }

        public string Code { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string DisasterType { get; set; } = null!;
    }
}