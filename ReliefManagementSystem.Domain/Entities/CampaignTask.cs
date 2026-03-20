using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class CampaignTask
    {
        public Guid CampaignTaskId { get; set; }

        public Guid CampaignTeamId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid CreatedBy { get; set; }

        public CampaignTaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public CampaignTeam CampaignTeam { get; set; } = default!;
        public ICollection<MemberTask> MemberTasks { get; set; } = new List<MemberTask>();
        public ICollection<CampaignTaskItem> CampaignTaskItems { get; set; } = new List<CampaignTaskItem>();
    }
}
