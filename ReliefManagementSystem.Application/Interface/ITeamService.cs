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
        Task<TeamResponse> CreateTeamAsync(CreateTeamRequest request, Guid moderatorId, CancellationToken cancellationToken);
        Task<TeamDetailResponse> GetTeamByIdAsync(Guid teamId, CancellationToken cancellationToken);
        Task<TeamResponse> UpdateTeamAsync(Guid teamId, UpdateTeamRequest request, Guid moderatorId, CancellationToken cancellationToken);
        Task<bool> DeleteTeamAsync(Guid teamId, Guid moderatorId, CancellationToken cancellationToken);
        Task<Pagination<TeamResponse>> GetAllTeamsAsync(int pageIndex, int pageSize, CancellationToken cancellationToken);
        Task<Pagination<TeamResponse>> SearchTeamsAsync(SearchTeamRequest request, CancellationToken cancellationToken);
        Task<List<TeamMemberInfo>> GetTeamMembersAsync(Guid teamId, CancellationToken cancellationToken);
        Task<bool> RemoveMemberAsync(Guid teamId, Guid userId, Guid moderatorId, CancellationToken cancellationToken);
        Task<List<TeamDetailResponse>> GetMyTeamsWithMembersAsync(Guid moderatorId, CancellationToken cancellationToken);
        Task<TeamDetailResponse> GetVolunteerTeamAsync(Guid userId, CancellationToken cancellationToken);
        Task<TeamMemberResponse> AddMemberDirectlyAsync(Guid teamId, AddMemberRequest request, Guid moderatorId, CancellationToken cancellationToken);
        Task<TeamMemberResponse> PromoteMemberToLeaderAsync(Guid teamId, Guid userId, Guid moderatorId, CancellationToken cancellationToken);
    }
}
