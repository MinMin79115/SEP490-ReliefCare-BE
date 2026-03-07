using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class MemberTask
    {
        public Guid MemberTaskId { get; set; }

        public Guid CampaignTaskId { get; set; }

        public Guid VolunteerProfileId { get; set; }

        public string SubTaskTitle { get; set; } = string.Empty;
        public string? TaskNote { get; set; }

        public DateTime? AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public MemberTaskStatus Status { get; set; }

        // Navigation
        public CampaignTask CampaignTask { get; set; } = default!;

        public VolunteerProfile VolunteerProfile { get; set; } = default!;

        public ICollection<MemberTaskItem> MemberTaskItems { get; set; } = new List<MemberTaskItem>();
    }
}
