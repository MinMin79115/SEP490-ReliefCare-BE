using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        ITeamRepository Teams { get; }
        ITeamMemberRepository TeamMembers { get; }
        ITeamJoinRequestRepository TeamJoinRequests { get; }
        IVolunteerProfileRepository VolunteerProfiles { get; }
        Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    }
}
