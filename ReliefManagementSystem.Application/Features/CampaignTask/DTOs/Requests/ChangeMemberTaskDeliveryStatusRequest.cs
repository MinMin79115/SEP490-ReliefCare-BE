using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests
{
    public class ChangeMemberTaskDeliveryStatusRequest
    {
        [Required]
        public MemberTaskStatus Status { get; set; }
        public string? Note { get; set; }
    }
}
