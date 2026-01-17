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
        Task<List<TeamJoinRequestResponse>> GetMyRequestsAsync(Guid volunteerId, CancellationToken cancellationToken);

        // Moderator actions
        //Task<TeamJoinRequestResponse> ReviewRequestAsync(Guid requestId, ReviewTeamJoinRequest review, Guid moderatorId, CancellationToken cancellationToken);
        Task<TeamJoinRequestResponse> ApproveRequestAsync(Guid requestId, Guid moderatorId, CancellationToken cancellationToken);
        Task<TeamJoinRequestResponse> RejectRequestAsync(Guid requestId,Guid moderatorId, CancellationToken cancellationToken);
        Task<List<TeamJoinRequestResponse>> GetPendingRequestsForMyTeamsAsync(Guid moderatorId, CancellationToken cancellationToken); 
        Task<List<TeamJoinRequestResponse>> GetRequestsByTeamAsync(Guid teamId, Guid moderatorId, CancellationToken cancellationToken); 

        // Common actions
        Task<TeamJoinRequestResponse> GetRequestByIdAsync(Guid requestId, CancellationToken cancellationToken);

    }
}
