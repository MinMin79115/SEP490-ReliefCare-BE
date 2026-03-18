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
        IEmailOtpRepository EmailOtps { get; }

        // Manager Profiles
        IManagerProfileRepository ManagerProfiles { get; }

        // Moderator Profiles
        IModeratorProfileRepository ModeratorProfiles { get; }
        
        ITeamRepository Teams { get; }
        ITeamMemberRepository TeamMembers { get; }
        ITeamJoinRequestRepository TeamJoinRequests { get; }
        IStationJoinRequestRepository StationJoinRequests { get; }

        // Volunteer Profiles
        IVolunteerProfileRepository VolunteerProfiles { get; }
        ISkillRepository Skills { get; }

        // Vehicle Management
        IVehicleRepository Vehicles { get; }
        IVehicleTypeRepository VehicleTypes { get; }

        // Inventory Management
        ISupplyItemRepository SupplyItems { get; }
        IInventoryRepository Inventories { get; }
        IInventoryStockRepository InventoryStocks { get; }
        IInventoryTransactionRepository InventoryTransactions { get; }

        // Relief Station Management
        IReliefStationTeamRepository ReliefStationTeams { get; }

        // Supply Allocation
        ISupplyAllocationRepository SupplyAllocations { get; }

        // Campaign (stub for validation — full module TBD)
        ICampaignRepository Campaigns { get; }

        // Relief Station Management
        IReliefStationRepository ReliefStations { get; }

        // Location Management
        ILocationRepository Locations { get; }

        IRescueOperationRepository RescueOperations { get; }
        IRescueRequestRepository RescueRequests { get; }
        IRescueRequestPriorityRepository RescueRequestPriorities { get; }
        IPriorityCriteriaRepository PriorityCriterias { get; }

        INotificationRepository Notifications { get; }
        Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    }
}
