using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.Team.Request
{
    public class SearchTeamRequest
    {
        public string? Name { get; set; }

        public TeamStatus? Status { get; set; }

        public Guid? ModeratorId { get; set; }

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
