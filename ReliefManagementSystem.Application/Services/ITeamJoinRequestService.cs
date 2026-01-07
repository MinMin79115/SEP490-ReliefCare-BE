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
        Task<TeamJoinRequestResponse> CreateRequestAsync(Guid volunteerId, CreateTeamJoinRequest request, CancellationToken cancellationToken);
        Task<bool> CancelRequestAsync(Guid volunteerId, CancellationToken cancellationToken);
        Task<List<TeamJoinRequestResponse>> GetMyRequestsAsync(Guid volunteerId, CancellationToken cancellationToken);

        // Moderator actions
        Task<TeamJoinRequestResponse> ReviewRequestAsync(Guid requestId, ReviewTeamJoinRequest review, Guid moderatorId, CancellationToken cancellationToken);
        Task<List<TeamJoinRequestResponse>> GetPendingRequestsForMyTeamAsync(Guid moderatorId, CancellationToken cancellationToken);
        Task<List<TeamJoinRequestResponse>> GetAllRequestsForMyTeamAsync(int teamId, Guid moderatorId, CancellationToken cancellationToken);

        // Common actions
        Task<TeamJoinRequestResponse> GetRequestByIdAsync(Guid requestId, CancellationToken cancellationToken);

    }
}
