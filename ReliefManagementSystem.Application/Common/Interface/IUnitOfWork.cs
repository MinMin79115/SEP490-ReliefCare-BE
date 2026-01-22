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

        // Inventory Management
        ISupplyItemRepository SupplyItems { get; }
        IInventoryTransactionRepository InventoryTransactions { get; }

        // Team Management
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
