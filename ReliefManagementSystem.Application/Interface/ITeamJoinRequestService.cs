using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.TeamJoinRequest.DTOs.Request;
using ReliefManagementSystem.Application.Features.TeamJoinRequest.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Interface
{
    public interface ITeamJoinRequestService
    {
        // Volunteer actions
        Task<TeamJoinRequestResponse> CreateRequestAsync(CreateTeamJoinRequest request, Guid volunteerId, CancellationToken cancellationToken); 
        Task<bool> CancelRequestAsync(Guid requestId, Guid volunteerId, CancellationToken cancellationToken); 
        Task<Pagination<TeamJoinRequestResponse>> GetMyRequestsAsync(Guid volunteerId, int pageIndex, int pageSize, CancellationToken cancellationToken);

        // Moderator actions
        Task<TeamJoinRequestResponse> ApproveRequestAsync(Guid requestId, Guid moderatorId, ReviewTeamJoinRequest request, CancellationToken cancellationToken);
        Task<TeamJoinRequestResponse> RejectRequestAsync(Guid requestId, Guid moderatorId, ReviewTeamJoinRequest request, CancellationToken cancellationToken);
        Task<Pagination<TeamJoinRequestResponse>> GetRequestsByTeamAsync(Guid teamId, Guid moderatorId, int pageIndex, int pageSize, CancellationToken cancellationToken); 

        // Common actions
        Task<TeamJoinRequestResponse> GetRequestByIdAsync(Guid requestId, CancellationToken cancellationToken);

    }
}
