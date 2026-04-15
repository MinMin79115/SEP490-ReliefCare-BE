using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;
using ReliefManagementSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
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
        private IDbContextTransaction? _currentTransaction;
        // User Management
        public IUserRepository Users { get; }
        public IRefreshTokenRepository RefreshTokens { get; }
        public IEmailOtpRepository EmailOtps { get; }

        // Manager Profiles
        public IManagerProfileRepository ManagerProfiles { get; }

        // Moderator Profiles
        public IModeratorProfileRepository ModeratorProfiles { get; }

        
        public ITeamRepository Teams { get; }
        public ITeamMemberRepository TeamMembers { get; }
        public ITeamTrackingPointRepository TeamTrackingPoints { get; }
        public ITeamJoinRequestRepository TeamJoinRequests { get; }
        public IStationJoinRequestRepository StationJoinRequests { get; }

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
        public ISupplyTransferRepository SupplyTransfers { get; }
        public IProcurementOrderRepository ProcurementOrders { get; }

        // Relief Station Management
        public IReliefStationRepository ReliefStations { get; }
        public IReliefStationTeamRepository ReliefStationTeams { get; }

        // Supply Allocation
        public ISupplyAllocationRepository SupplyAllocations { get; }

        // Relief Distribution MVP
        public ICampaignHouseholdRepository CampaignHouseholds { get; }
        public IDistributionPointRepository DistributionPoints { get; }
        public IReliefPackageDefinitionRepository ReliefPackageDefinitions { get; }
        public IReliefPackageDefinitionItemRepository ReliefPackageDefinitionItems { get; }
        public IReliefPackageAssemblyRepository ReliefPackageAssemblies { get; }
        public IReliefPackageAssemblyDetailRepository ReliefPackageAssemblyDetails { get; }
        public IHouseholdDeliveryRepository HouseholdDeliveries { get; }
        public IHouseholdDeliveryProofRepository HouseholdDeliveryProofs { get; }
        public ISupplyShortageRequestRepository SupplyShortageRequests { get; }
        public ISupplyShortageRequestItemRepository SupplyShortageRequestItems { get; }

        // Campaign (stub for validation — full module TBD)
        public ICampaignRepository Campaigns { get; }
        public ICampaignVolunteerRegistrationRepository CampaignVolunteerRegistrations { get; }
        public IDonationRepository Donations { get; }
        public IFundRepository Funds { get; }
        public IPaymentTransactionRepository PaymentTransactions { get; }


        //Location Management
        public ILocationRepository Locations { get; }

        public IRescueRequestRepository RescueRequests { get; }

        public IRescueBatchRepository RescueBatches { get; }
        public IRescueBatchItemRepository RescueBatchItems { get; }
        public IPriorityCriteriaRepository PriorityCriterias { get; }
        public IRescueRequestPriorityRepository RescueRequestPriorities { get; }

        public IRescueOperationRepository RescueOperations { get; }

        public INotificationRepository Notifications { get; }
        public IAttachmentRepository Attachments { get; }
        public IGenericRepository<DisasterAnalysisLog> DisasterAnalysisLogs { get; }

        // Constructor
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            RefreshTokens = new RefreshTokenRepository(_context);
            EmailOtps = new EmailOtpRepository(_context);
            ManagerProfiles = new ManagerProfileRepository(_context);
            ModeratorProfiles = new ModeratorProfileRepository(_context);


            Teams = new TeamRepository(_context);
            TeamMembers = new TeamMemberRepository(_context);
            TeamTrackingPoints = new TeamTrackingPointRepository(_context);
            TeamJoinRequests = new TeamJoinRequestRepository(_context);
            StationJoinRequests = new StationJoinRequestRepository(_context);
            VolunteerProfiles = new VolunteerProfileRepository(_context);
            Skills = new SkillRepository(_context);
            Vehicles = new VehicleRepository(_context);
            VehicleTypes = new VehicleTypeRepository(_context);
            SupplyItems = new SupplyItemRepository(_context);
            Inventories = new InventoryRepository(_context);
            InventoryStocks = new InventoryStockRepository(_context);
            InventoryTransactions = new InventoryTransactionRepository(_context);
            SupplyTransfers = new SupplyTransferRepository(_context);
            ProcurementOrders = new ProcurementOrderRepository(_context);
            ReliefStationTeams = new ReliefStationTeamRepository(_context);
            SupplyAllocations = new SupplyAllocationRepository(_context);
            CampaignHouseholds = new CampaignHouseholdRepository(_context);
            DistributionPoints = new DistributionPointRepository(_context);
            ReliefPackageDefinitions = new ReliefPackageDefinitionRepository(_context);
            ReliefPackageDefinitionItems = new ReliefPackageDefinitionItemRepository(_context);
            ReliefPackageAssemblies = new ReliefPackageAssemblyRepository(_context);
            ReliefPackageAssemblyDetails = new ReliefPackageAssemblyDetailRepository(_context);
            HouseholdDeliveries = new HouseholdDeliveryRepository(_context);
            HouseholdDeliveryProofs = new HouseholdDeliveryProofRepository(_context);
            SupplyShortageRequests = new SupplyShortageRequestRepository(_context);
            SupplyShortageRequestItems = new SupplyShortageRequestItemRepository(_context);
            Campaigns = new CampaignRepository(_context);
            CampaignVolunteerRegistrations = new CampaignVolunteerRegistrationRepository(_context);
            Donations = new DonationRepository(_context);
            Funds = new FundRepository(_context);
            PaymentTransactions = new PaymentTransactionRepository(_context);
            ReliefStations = new ReliefStationRepository(_context);
            Locations = new LocationRepository(_context);
            RescueRequests = new RescueRequestRepository(_context);

            RescueBatches = new RescueBatchRepository(_context);
            RescueBatchItems = new RescueBatchItemRepository(_context);
            PriorityCriterias = new PriorityCriteriaRepository(_context);
            RescueRequestPriorities = new RescueRequestPriorityRepository(_context);
            RescueOperations = new RescueOperationRepository(_context);
            Notifications = new NotificationRepository(_context);
            Attachments = new AttachmentRepository(_context);
            DisasterAnalysisLogs = new GenericRepository<DisasterAnalysisLog>(_context);
        }

        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(
            CancellationToken cancellationToken = default,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                return;
            }

            await _currentTransaction.CommitAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null)
            {
                return;
            }

            await _currentTransaction.RollbackAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }
    }
}
