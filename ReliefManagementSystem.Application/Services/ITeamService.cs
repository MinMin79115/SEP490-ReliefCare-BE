using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Team.Request;
using ReliefManagementSystem.Application.Features.Team.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Services
{
    public interface ITeamService
    {
        // CRUD Operations
        Task<TeamResponse> CreateTeamAsync(CreateTeamRequest request, Guid moderatorId, CancellationToken cancellationToken);
        Task<TeamDetailResponse> GetTeamByIdAsync(int teamId, CancellationToken cancellationToken);
        Task<TeamResponse> UpdateTeamAsync(int teamId, UpdateTeamRequest request, Guid moderatorId, CancellationToken cancellationToken);
        Task<bool> DeleteTeamAsync(int teamId, Guid moderatorId, CancellationToken cancellationToken);

        // List & Search
        Task<List<TeamResponse>> GetAllTeamsAsync(CancellationToken cancellationToken);
        Task<Pagination<TeamResponse>> SearchTeamAsync(SearchTeamRequest request, CancellationToken cancellationToken);
        Task<List<TeamResponse>> GetMyTeamsAsync(Guid moderatorId, CancellationToken cancellationToken);

        // Member Management
        Task<List<TeamMemberInfo>> GetTeamMembersAsync(int teamId, CancellationToken cancellationToken);
        Task<bool> RemoveMemberAsync(int teamId, Guid userId, Guid moderatorId, CancellationToken cancellationToken);



    }
}
