using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        // User Management
        IUserRepository Users { get; }
        IRefreshTokenRepository RefreshTokens { get; }


        
        ITeamRepository Teams { get; }
        ITeamMemberRepository TeamMembers { get; }
        ITeamJoinRequestRepository TeamJoinRequests { get; }

        // Volunteer Profiles
        IVolunteerProfileRepository VolunteerProfiles { get; }
        ISkillRepository Skills { get; }

        // Vehicle Management
        IVehicleRepository Vehicles { get; }
        IVehicleTypeRepository VehicleTypes { get; }
        Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    }
}
