using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request
{
    public class CompleteRescueOperationRequestDto
    {
        [Required]
        [MinLength(1)]
        public List<AttachmentItem> Attachments { get; set; } = new();

        public string? Note { get; set; }

        public class AttachmentItem
        {
            [Required]
            public string FileUrl { get; set; } = null!;

            [Required]
            public string ContentType { get; set; } = null!;
        }
    }
}
