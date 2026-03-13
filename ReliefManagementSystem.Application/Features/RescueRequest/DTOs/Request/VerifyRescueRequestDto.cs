using ReliefManagementSystem.Domain.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request
{
    /// <summary>DTO để xác minh yêu cầu cứu hộ (cho Admin/Manager)</summary>
    public class VerifyRescueRequestDto
    {
        public RequestVerificationStatus Status { get; set; }

        public VerificationMethod Method { get; set; }

        public string? Note { get; set; }

        public string? Reason { get; set; }
    }
}