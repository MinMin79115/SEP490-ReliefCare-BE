using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request
{
    public class RejectRescueRequestDto
    {
        public RequestVerificationStatus Status { get; set; }
        public VerificationMethod Method { get; set; }
        public string? Reason { get; set; }
    }
}
