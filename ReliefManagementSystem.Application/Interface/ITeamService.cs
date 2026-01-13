using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Team.DTOs.Request;
using ReliefManagementSystem.Application.Features.Team.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Interface
{
    public interface ITeamService
    {
        // CRUD Operations
        Task<TeamResponse> CreateTeamAsync(CreateTeamRequest request, Guid moderatorId, CancellationToken cancellationToken);
        Task<TeamDetailResponse> GetTeamByIdAsync(Guid teamId, CancellationToken cancellationToken);
        Task<TeamResponse> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, Guid moderatorId, CancellationToken cancellationToken);
        Task<bool> DeleteTeamAsync(Guid teamId, Guid moderatorId, CancellationToken cancellationToken);

        // List & Search
        Task<List<TeamResponse>> GetAllTeamsAsync(CancellationToken cancellationToken);
        Task<Pagination<TeamResponse>> SearchTeamsAsync(SearchTeamRequest request, CancellationToken cancellationToken);
        Task<List<TeamResponse>> GetMyTeamsAsync(Guid moderatorId, CancellationToken cancellationToken);

        // Member Management
        Task<List<TeamMemberInfo>> GetTeamMembersAsync(Guid teamId, CancellationToken cancellationToken);
        Task<bool> RemoveMemberAsync(Guid teamId, Guid userId, Guid moderatorId, CancellationToken cancellationToken);



    }
}
