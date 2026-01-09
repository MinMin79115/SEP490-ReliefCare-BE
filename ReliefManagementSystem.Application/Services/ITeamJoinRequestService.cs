using ReliefManagementSystem.Application.Features.TeamJoinRequest.Request;
using ReliefManagementSystem.Application.Features.TeamJoinRequest.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Services
{
    public interface ITeamJoinRequestService
    {
        // Volunteer actions
        Task<TeamJoinRequestResponse> CreateRequestAsync(CreateTeamJoinRequest request, Guid volunteerId, CancellationToken cancellationToken); 
        Task<bool> CancelRequestAsync(Guid requestId, Guid volunteerId, CancellationToken cancellationToken); 
        Task<List<TeamJoinRequestResponse>> GetMyRequestsAsync(Guid volunteerId, CancellationToken cancellationToken);

        // Moderator actions
        Task<TeamJoinRequestResponse> ReviewRequestAsync(Guid requestId, ReviewTeamJoinRequest review, Guid moderatorId, CancellationToken cancellationToken);
        Task<List<TeamJoinRequestResponse>> GetPendingRequestsForMyTeamsAsync(Guid moderatorId, CancellationToken cancellationToken); 
        Task<List<TeamJoinRequestResponse>> GetRequestsByTeamAsync(Guid teamId, Guid moderatorId, CancellationToken cancellationToken); 

        // Common actions
        Task<TeamJoinRequestResponse> GetRequestByIdAsync(Guid requestId, CancellationToken cancellationToken);

    }
}
