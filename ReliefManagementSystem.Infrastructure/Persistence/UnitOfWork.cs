using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Infrastructure.Data;
using ReliefManagementSystem.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        // User Management
        public IUserRepository Users { get; }
        public IRefreshTokenRepository RefreshTokens { get; }

        
        public ITeamRepository Teams { get; }
        public ITeamMemberRepository TeamMembers { get; }
        public ITeamJoinRequestRepository TeamJoinRequests { get; }

        // Volunteer Profiles
        public IVolunteerProfileRepository VolunteerProfiles { get; }

        public ISkillRepository Skills { get; }

        // Vehicle Management
        public IVehicleRepository Vehicles { get; }
        public IVehicleTypeRepository VehicleTypes { get; }

        // Inventory Management
        public ISupplyItemRepository SupplyItems { get; }
        public IInventoryRepository Inventories { get; }
        public IInventoryStockRepository InventoryStocks { get; }
        public IInventoryTransactionRepository InventoryTransactions { get; }

        // Relief Station Management
        public IReliefStationRepository ReliefStations { get; }
        public IReliefStationTeamRepository ReliefStationTeams { get; }

        // Supply Allocation
        public ISupplyAllocationRepository SupplyAllocations { get; }

        // Campaign (stub for validation — full module TBD)
        public ICampaignRepository Campaigns { get; }

        // Relief Station Management
        public IReliefStationRepository ReliefStations { get; }

        //Location Management
        public ILocationRepository Locations { get; }

        // Constructor
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            RefreshTokens = new RefreshTokenRepository(_context);


            Teams = new TeamRepository(_context);
            TeamMembers = new TeamMemberRepository(_context);
            TeamJoinRequests = new TeamJoinRequestRepository(_context);
            VolunteerProfiles = new VolunteerProfileRepository(_context);
            Skills = new SkillRepository(_context);
            Vehicles = new VehicleRepository(_context);
            VehicleTypes = new VehicleTypeRepository(_context);
            SupplyItems = new SupplyItemRepository(_context);
            Inventories = new InventoryRepository(_context);
            InventoryStocks = new InventoryStockRepository(_context);
            InventoryTransactions = new InventoryTransactionRepository(_context);
            ReliefStations = new ReliefStationRepository(_context);
            ReliefStationTeams = new ReliefStationTeamRepository(_context);
            SupplyAllocations = new SupplyAllocationRepository(_context);
            Campaigns = new CampaignRepository(_context);
            ReliefStations = new ReliefStationRepository(_context);
            Locations = new LocationRepository(_context);
        }

        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
