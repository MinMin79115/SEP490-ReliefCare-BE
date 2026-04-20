using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Entities.Common;
using ReliefManagementSystem.Application.Interface;
using System.Text.Json;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<VolunteerProfile> VolunteerProfiles { get; set; }
        public DbSet<ManagerProfile> ManagerProfiles { get; set; }
        public DbSet<ModeratorProfile> ModeratorProfiles { get; set; }

        // Donation
        public DbSet<Donation> Donations { get; set; }
        public DbSet<InKindDonation> InKindDonations { get; set; }
        public DbSet<InKindDonationDetail> InKindDonationDetails { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<PaymentTransactionDetail> PaymentTransactionDetails { get; set; }
        public DbSet<Fund> Funds { get; set; }
        public DbSet<FundContribution> FundContributions { get; set; }
        public DbSet<FundTransaction> FundTransactions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<VolunteerSkill> VolunteerSkills { get; set; }
        public DbSet<VolunteerCertificate> VolunteerCertificates { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<TeamJoinRequest> TeamJoinRequests { get; set; }
        public DbSet<StationJoinRequest> StationJoinRequests { get; set; }
        public DbSet<EmailOtp> EmailOtps { get; set; }


        // Vehicle Management
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }

        // Inventory Management
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryStock> InventoryStocks { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<ProcurementOrder> ProcurementOrders { get; set; }
        public DbSet<ProcurementOrderItem> ProcurementOrderItems { get; set; }
        public DbSet<InventoryTransactionItem> InventoryTransactionItems { get; set; }
        public DbSet<SupplyItem> SupplyItems { get; set; }
        public DbSet<ReliefStation> ReliefStations { get; set; }
        public DbSet<ReliefStationTeam> ReliefStationTeams { get; set; }

        // Location
        public DbSet<Location> Locations { get; set; }

        // Campaign Management
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignInventory> CampaignInventories { get; set; }
        public DbSet<CampaignInventoryStock> CampaignInventoryStocks { get; set; }
        public DbSet<CampaignInventoryTransaction> CampaignInventoryTransactions { get; set; }
        public DbSet<CampaignInventoryTransactionItem> CampaignInventoryTransactionItems { get; set; }
        public DbSet<CampaignResourceGoal> CampaignResourceGoals { get; set; }
        public DbSet<CampaignStation> CampaignStations { get; set; }
        public DbSet<CampaignTeam> CampaignTeams { get; set; }
        public DbSet<CampaignVolunteerRegistration> CampaignVolunteerRegistrations { get; set; }
        public DbSet<CampaignTask> CampaignTasks { get; set; }
        public DbSet<CampaignVehicle> CampaignVehicles { get; set; }
        public DbSet<MemberTask> MemberTasks { get; set; }
        
        public DbSet<CampaignTaskItem> CampaignTaskItems { get; set; }
        public DbSet<MemberTaskItem> MemberTaskItems { get; set; }

        // Relief Distribution MVP
        public DbSet<CampaignHousehold> CampaignHouseholds { get; set; }
        public DbSet<DistributionPoint> DistributionPoints { get; set; }
        public DbSet<ReliefPackageDefinition> ReliefPackageDefinitions { get; set; }
        public DbSet<ReliefPackageDefinitionItem> ReliefPackageDefinitionItems { get; set; }
        public DbSet<ReliefPackageAssembly> ReliefPackageAssemblies { get; set; }
        public DbSet<ReliefPackageAssemblyDetail> ReliefPackageAssemblyDetails { get; set; }
        public DbSet<HouseholdDelivery> HouseholdDeliveries { get; set; }
        public DbSet<HouseholdDeliveryProof> HouseholdDeliveryProofs { get; set; }
        public DbSet<SupplyShortageRequest> SupplyShortageRequests { get; set; }
        public DbSet<SupplyShortageRequestItem> SupplyShortageRequestItems { get; set; }

        // Supply Allocation
        public DbSet<SupplyAllocation> SupplyAllocations { get; set; }
        public DbSet<SupplyAllocationItem> SupplyAllocationItems { get; set; }

        // Supply Transfer (vận chuyển hàng giữa các trạm)
        public DbSet<SupplyTransfer> SupplyTransfers { get; set; }
        public DbSet<SupplyTransferDocument> SupplyTransferDocuments { get; set; }
        public DbSet<SupplyTransferItem> SupplyTransferItems { get; set; }

        // Notification (thông báo real-time)
        public DbSet<Notification> Notifications { get; set; }

        //Request Management
        public DbSet<Request> Requests { get; set; }
        public DbSet<RescueRequest> RescueRequests { get; set; }
        public DbSet<PriorityCriteria> PriorityCriterias { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<RequestVerification> RequestVerifications { get; set; }
        public DbSet<RescueOperation> RescueOperations { get; set; }
        public DbSet<RescueRequestPriority> RescueRequestPriorities { get; set; }
        public DbSet<RescueBatch> RescueBatches { get; set; }
        public DbSet<RescueBatchItem> RescueBatchItems { get; set; }
        public DbSet<TeamTrackingPoint> TeamTrackingPoints { get; set; }
        public DbSet<DisasterAnalysisLog> DisasterAnalysisLogs { get; set; }

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService? currentUserService = null)
          : base(options) 
        {
            _currentUserService = currentUserService;
        }

        private readonly ICurrentUserService? _currentUserService;

        public override int SaveChanges()
        {
            return SaveChangesAsync().GetAwaiter().GetResult();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            NormalizeDateTimeKindsToUtc();

            var userId = _currentUserService?.UserId;
            var auditLogs = new List<AuditLog>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                // 1. AuditableEntity Tracking
                if (entry.Entity is AuditableEntity auditableEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditableEntity.CreatedAt = DateTime.UtcNow;
                            auditableEntity.CreatedBy = userId;
                            break;
                        case EntityState.Modified:
                            auditableEntity.UpdatedAt = DateTime.UtcNow;
                            auditableEntity.UpdatedBy = userId;
                            break;
                        case EntityState.Deleted:
                            entry.State = EntityState.Modified;
                            auditableEntity.IsDeleted = true;
                            auditableEntity.UpdatedAt = DateTime.UtcNow;
                            auditableEntity.UpdatedBy = userId;
                            break;
                    }
                }

                // 2. AuditLog History Recording
                if (entry.Entity is AuditLog)
                    continue;

                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();

                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary) continue;
                    string propertyName = property.Metadata.Name;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            newValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            oldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                oldValues[propertyName] = property.OriginalValue;
                                newValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                if (oldValues.Count > 0 || newValues.Count > 0 || entry.State == EntityState.Deleted || entry.State == EntityState.Added)
                {
                    var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
                    
                    auditLogs.Add(new AuditLog
                    {
                        EntityName = entry.Entity.GetType().Name,
                        Action = entry.State.ToString(),
                        Timestamp = DateTime.UtcNow,
                        UserId = userId,
                        PrimaryKey = primaryKey?.CurrentValue?.ToString(),
                        OldValues = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null,
                        NewValues = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null
                    });
                }
            }

            if (auditLogs.Count > 0)
            {
                AuditLogs.AddRange(auditLogs);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        private void NormalizeDateTimeKindsToUtc()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
                    continue;

                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(DateTime) && property.CurrentValue is DateTime dt)
                    {
                        property.CurrentValue = EnsureUtc(dt);
                    }
                    else if (property.Metadata.ClrType == typeof(DateTime?) && property.CurrentValue is DateTime ndt)
                    {
                        property.CurrentValue = EnsureUtc(ndt);
                    }
                }
            }
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ManagerProfile configuration (1:1 với ApplicationUser)
            builder.Entity<ManagerProfile>()
                .HasKey(mp => mp.ManagerProfileId);

            builder.Entity<ManagerProfile>()
                .HasOne(mp => mp.User)
                .WithOne(u => u.ManagerProfile)
                .HasForeignKey<ManagerProfile>(mp => mp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ManagerProfile>()
                .HasOne(mp => mp.AssignedLocation)
                .WithMany()
                .HasForeignKey(mp => mp.AssignedLocationId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ManagerProfile>()
                .Property(mp => mp.Level)
                .HasConversion<string>()
                .IsRequired();

            // Fund configuration
            builder.Entity<Fund>()
                .HasKey(f => f.FundId);

            builder.Entity<Fund>()
                .HasIndex(f => f.IsDefault)
                .HasFilter("\"IsDefault\" = true");

            builder.Entity<FundContribution>()
                .HasKey(fc => fc.FundContributionId);

            builder.Entity<FundContribution>()
                .HasOne(fc => fc.Fund)
                .WithMany(f => f.Contributions)
                .HasForeignKey(fc => fc.FundId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FundContribution>()
                .HasOne(fc => fc.Donation)
                .WithMany(d => d.FundContributions)
                .HasForeignKey(fc => fc.DonationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FundContribution>()
                .HasOne(fc => fc.Campaign)
                .WithMany()
                .HasForeignKey(fc => fc.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<FundContribution>()
                .HasIndex(fc => fc.DonationId)
                .IsUnique();

            builder.Entity<FundTransaction>()
                .HasKey(ft => ft.FundTransactionId);

            builder.Entity<FundTransaction>()
                .Property(ft => ft.Type)
                .HasConversion<string>()
                .IsRequired();

            builder.Entity<CampaignVolunteerRegistration>()
                .HasKey(x => x.CampaignVolunteerRegistrationId);

            builder.Entity<CampaignVolunteerRegistration>()
                .Property(x => x.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Entity<CampaignVolunteerRegistration>()
                .HasOne(x => x.Campaign)
                .WithMany(c => c.VolunteerRegistrations)
                .HasForeignKey(x => x.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CampaignVolunteerRegistration>()
                .HasOne(x => x.User)
                .WithMany(u => u.CampaignVolunteerRegistrations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CampaignVolunteerRegistration>()
                .HasIndex(x => new { x.CampaignId, x.UserId, x.Status });

            builder.Entity<FundTransaction>()
                .HasOne(ft => ft.Fund)
                .WithMany(f => f.Transactions)
                .HasForeignKey(ft => ft.FundId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FundTransaction>()
                .HasOne(ft => ft.FundContribution)
                .WithMany()
                .HasForeignKey(ft => ft.FundContributionId)
                .OnDelete(DeleteBehavior.SetNull);


            // ModeratorProfile configuration (1:1 với ApplicationUser)
            builder.Entity<ModeratorProfile>()
                .HasKey(mp => mp.ModeratorProfileId);

            builder.Entity<ModeratorProfile>()
                .HasOne(mp => mp.User)
                .WithOne(u => u.ModeratorProfile)
                .HasForeignKey<ModeratorProfile>(mp => mp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ModeratorProfile → ReliefStation (nhiều Moderator có thể thuộc 1 trạm)
            builder.Entity<ModeratorProfile>()
                .HasOne(mp => mp.ReliefStation)
                .WithMany(rs => rs.Moderators)
                .HasForeignKey(mp => mp.ReliefStationId)
                .OnDelete(DeleteBehavior.SetNull);

            // VolunteerProfile configuration

            builder.Entity<VolunteerProfile>()
                .HasKey(v => v.VolunteerProfileId);

            builder.Entity<VolunteerProfile>()
                .HasOne(v => v.User)
                .WithOne(u => u.VolunteerProfile)
                .HasForeignKey<VolunteerProfile>(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // VolunteerCertificate configuration
            builder.Entity<VolunteerCertificate>()
                .HasKey(c => c.CertificateId);

            builder.Entity<VolunteerCertificate>()
                .HasOne(c => c.VolunteerProfile)
                .WithMany(vp => vp.Certificates)
                .HasForeignKey(c => c.VolunteerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<VolunteerCertificate>()
                .Property(c => c.Name)
                .HasMaxLength(200)
                .IsRequired();

            //VolunteerSkill configuration
            builder.Entity<VolunteerSkill>()
                .HasKey(vs => new { vs.VolunteerProfileId, vs.SkillId });

            builder.Entity<VolunteerSkill>()
                .HasOne(vs => vs.VolunteerProfile)
                .WithMany(vp => vp.VolunteerSkills)
                .HasForeignKey(vs => vs.VolunteerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<VolunteerSkill>()
                .HasOne(vs => vs.Skill)
                .WithMany(s => s.VolunteerSkills)
                .HasForeignKey(vs => vs.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            //TeamMember configuration
            builder.Entity<TeamMember>()
                .HasKey(tm => new { tm.TeamId, tm.UserId });

            builder.Entity<TeamMember>()
                .HasOne(tm => tm.Team)
                .WithMany(t => t.TeamMembers)
                .HasForeignKey(tm => tm.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TeamMember>()
                .HasOne(tm => tm.User)
                .WithMany(u => u.TeamMembers)
                .HasForeignKey(tm => tm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //Team Configuration
            builder.Entity<Team>()
                .HasOne(t => t.Leader)
                .WithMany()
                .HasForeignKey(t => t.LeaderId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Team>()
                .HasOne(t => t.Moderator)
                .WithMany()
                .HasForeignKey(t => t.CreateBy)
                .OnDelete(DeleteBehavior.Restrict);

            //TeamJoinRequest Configuration
            builder.Entity<TeamJoinRequest>()
                .HasOne(tjr => tjr.Team)
                .WithMany(t => t.TeamJoinRequests)
                .HasForeignKey(tjr => tjr.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TeamJoinRequest>()
                .HasOne(tjr => tjr.Volunteer)
                .WithMany()
                .HasForeignKey(tjr => tjr.VolunteerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TeamJoinRequest>()
                .HasOne(tjr => tjr.Reviewer)
                .WithMany()
                .HasForeignKey(tjr => tjr.ReviewedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // StationJoinRequest Configuration
            builder.Entity<StationJoinRequest>()
                .HasOne(sjr => sjr.Team)
                .WithMany(t => t.StationJoinRequests)
                .HasForeignKey(sjr => sjr.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StationJoinRequest>()
                .HasOne(sjr => sjr.ReliefStation)
                .WithMany(rs => rs.StationJoinRequests)
                .HasForeignKey(sjr => sjr.ReliefStationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StationJoinRequest>()
                .HasOne(sjr => sjr.RequestedByLeader)
                .WithMany()
                .HasForeignKey(sjr => sjr.RequestedByLeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StationJoinRequest>()
                .HasOne(sjr => sjr.ReviewedByModerator)
                .WithMany()
                .HasForeignKey(sjr => sjr.ReviewedByModeratorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<StationJoinRequest>()
                .HasIndex(sjr => new { sjr.TeamId, sjr.ReliefStationId, sjr.Status });

            builder.Entity<EmailOtp>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EmailOtp>()
                .Property(x => x.CodeHash)
                .HasMaxLength(256)
                .IsRequired();

            builder.Entity<EmailOtp>()
                .HasIndex(x => new { x.UserId, x.Purpose, x.CreatedAt });

            //RefreshToken Configuration
            builder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);




            // Vehicle Management Configurations
            builder.Entity<Vehicle>()
    .HasOne(v => v.Creator)
    .WithMany()
    .HasForeignKey(v => v.CreatedBy)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vehicle>()
                .HasOne(v => v.VehicleType)
                .WithMany(vt => vt.Vehicles)
                .HasForeignKey(v => v.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            builder.Entity<Vehicle>()
                .Property(v => v.LicensePlate)
                .HasMaxLength(20)
                .IsRequired();

            // VehicleType Configuration
            builder.Entity<VehicleType>()
                .Property(vt => vt.TypeName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Entity<VehicleType>()
                .Property(vt => vt.CapacityUnit)
                .HasMaxLength(20)
                .IsRequired();

            builder.Entity<VehicleType>()
                .HasIndex(vt => vt.TypeName)
                .IsUnique();

            builder.Entity<Vehicle>()
                .HasOne(v => v.ReliefStation)
                .WithMany(rs => rs.Vehicles)
                .HasForeignKey(v => v.ReliefStationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Vehicle>()
                .HasOne(v => v.Team)
                .WithMany()
                .HasForeignKey(v => v.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            // Inventory Management Configurations

            // InventoryStock Configuration - Unique constraint on (InventoryId, SupplyItemId)
            builder.Entity<InventoryStock>()
                .HasKey(i => i.InventoryStockId);

            builder.Entity<InventoryStock>()
                .ToTable(t => t.HasCheckConstraint("CK_InventoryStocks_CurrentQuantity_NonNegative", "\"CurrentQuantity\" >= 0"));

            builder.Entity<InventoryStock>()
                .HasIndex(i => new { i.InventoryId, i.SupplyItemId })
                .IsUnique();

            builder.Entity<InventoryStock>()
                .HasOne(i => i.Inventory)
                .WithMany(inv => inv.InventoryItems)
                .HasForeignKey(i => i.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InventoryStock>()
                .HasOne(i => i.SupplyItem)
                .WithMany(s => s.InventoryItems)
                .HasForeignKey(i => i.SupplyItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Inventory Configuration
            builder.Entity<Inventory>()
                .HasKey(i => i.InventoryId);

            builder.Entity<Inventory>()
                .HasOne(i => i.ReliefStation)
                .WithMany(rs => rs.Inventories)
                .HasForeignKey(i => i.ReliefStationId)
                .OnDelete(DeleteBehavior.Restrict);

            // SupplyItem Configuration
            builder.Entity<SupplyItem>()
                .HasKey(s => s.SupplyItemId);

            // InventoryTransaction Configuration
            builder.Entity<InventoryTransaction>()
                .HasKey(it => it.TransactionId);

            builder.Entity<InventoryTransaction>()
                .HasIndex(it => it.TransactionCode)
                .IsUnique();

            builder.Entity<InventoryTransaction>()
                .HasOne(it => it.Inventory)
                .WithMany(i => i.InventoryTransactions)
                .HasForeignKey(it => it.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InventoryTransaction>()
                .HasOne(it => it.CreatedByUser)
                .WithMany()
                .HasForeignKey(it => it.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryTransaction>()
                .HasOne(it => it.SupplyTransfer)
                .WithMany(st => st.InventoryTransactions)
                .HasForeignKey(it => it.SupplyTransferId)
                .OnDelete(DeleteBehavior.Restrict);

            // ReliefStation Configuration
            builder.Entity<ReliefStation>()
                .HasKey(rs => rs.ReliefStationId);

            // ReliefStationTeam Configuration (Many-to-Many between ReliefStation and Team)
            builder.Entity<ReliefStationTeam>()
                .HasKey(rst => rst.ReliefStationTeamId);

            builder.Entity<ReliefStationTeam>()
                .HasOne(rst => rst.ReliefStation)
                .WithMany(rs => rs.ReliefStationTeams)
                .HasForeignKey(rst => rst.ReliefStationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ReliefStationTeam>()
                .HasOne(rst => rst.Team)
                .WithMany(rst => rst.ReliefStationTeams)
                .HasForeignKey(rst => rst.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ReliefStationTeam>()
                .HasIndex(rst => new { rst.ReliefStationId, rst.TeamId })
                .IsUnique();

            // InventoryTransactionItem Configuration
            builder.Entity<InventoryTransactionItem>()
                .HasKey(iti => iti.TransactionItemId);

            builder.Entity<InventoryTransactionItem>()
                .HasOne(iti => iti.Transaction)
                .WithMany(t => t.Items)
                .HasForeignKey(iti => iti.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InventoryTransactionItem>()
                .HasOne(iti => iti.SupplyItem)
                .WithMany(s => s.InventoryTransactionItems)
                .HasForeignKey(iti => iti.SupplyItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Location Configuration
            builder.Entity<Location>()
                .HasKey(l => l.LocationId);

            builder.Entity<Location>()
                .HasOne(l => l.Parent)
                .WithMany(l => l.Children)
                .HasForeignKey(l => l.ParentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Location>()
                .HasIndex(l => l.ParentId);

            // Campaign Configuration
            builder.Entity<Campaign>()
                .HasKey(c => c.CampaignId);

            builder.Entity<Campaign>()
                .HasOne(c => c.Location)
                .WithMany()
                .HasForeignKey(c => c.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Campaign>()
                .Property(c => c.Type)
                .HasConversion<string>()
                .IsRequired();

            builder.Entity<Campaign>()
                .Property(c => c.CompletionRule)
                .HasConversion<string>()
                .IsRequired();

            builder.Entity<Campaign>()
                .Property(c => c.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Entity<Campaign>()
                .Property(c => c.AllowOverTarget)
                .HasDefaultValue(true);

            // CampaignResourceGoal Configuration
            builder.Entity<CampaignResourceGoal>()
                .HasKey(g => g.CampaignResourceGoalId);

            builder.Entity<CampaignResourceGoal>()
                .Property(g => g.ResourceType)
                .HasConversion<string>()
                .IsRequired();

            builder.Entity<CampaignResourceGoal>()
                .Property(g => g.IsRequired)
                .HasDefaultValue(true);

            builder.Entity<CampaignResourceGoal>()
                .HasOne(g => g.Campaign)
                .WithMany(c => c.ResourceGoals)
                .HasForeignKey(g => g.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CampaignResourceGoal>()
                .HasIndex(g => new { g.CampaignId, g.ResourceType })
                .IsUnique();

            builder.Entity<ProcurementOrder>()
                .HasKey(p => p.ProcurementOrderId);

            builder.Entity<ProcurementOrder>()
                .Property(p => p.Status)
                .HasConversion<string>()
                .IsRequired();

            builder.Entity<ProcurementOrder>()
                .HasOne(p => p.Campaign)
                .WithMany()
                .HasForeignKey(p => p.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProcurementOrder>()
                .HasOne(p => p.DestinationInventory)
                .WithMany()
                .HasForeignKey(p => p.DestinationInventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProcurementOrder>()
                .HasOne(p => p.InventoryTransaction)
                .WithMany()
                .HasForeignKey(p => p.InventoryTransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ProcurementOrderItem>()
                .HasKey(i => i.ProcurementOrderItemId);

            builder.Entity<ProcurementOrderItem>()
                .HasOne(i => i.ProcurementOrder)
                .WithMany(p => p.Items)
                .HasForeignKey(i => i.ProcurementOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProcurementOrderItem>()
                .HasOne(i => i.SupplyItem)
                .WithMany()
                .HasForeignKey(i => i.SupplyItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // CampaignStation Configuration
            builder.Entity<CampaignStation>()
                .HasKey(cs => new { cs.CampaignId, cs.ReliefStationId });

            builder.Entity<CampaignStation>()
                .HasOne(cs => cs.Campaign)
                .WithMany(c => c.CampaignStations)
                .HasForeignKey(cs => cs.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CampaignStation>()
                .HasOne(cs => cs.ReliefStation)
                .WithMany(rs => rs.CampaignStations)
                .HasForeignKey(cs => cs.ReliefStationId)
                .OnDelete(DeleteBehavior.Cascade);

            // CampaignTeam Configuration
            builder.Entity<CampaignTeam>()
                .HasKey(ct => ct.CampaignTeamId);

            builder.Entity<CampaignTeam>()
                .HasOne(ct => ct.Campaign)
                .WithMany(c => c.CampaignTeams)
                .HasForeignKey(ct => ct.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CampaignTeam>()
                .HasOne(ct => ct.Team)
                .WithMany(t => t.CampaignTeams)
                .HasForeignKey(ct => ct.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CampaignTeam>()
                .HasIndex(ct => new { ct.CampaignId, ct.TeamId })
                .IsUnique();

            // CampaignInventory Configuration
            builder.Entity<CampaignInventory>()
                .HasKey(ci => ci.CampaignInventoryId);

            builder.Entity<CampaignInventory>()
                .HasOne(ci => ci.Campaign)
                .WithOne(c => c.CampaignInventory)
                .HasForeignKey<CampaignInventory>(ci => ci.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CampaignInventory>()
                .HasIndex(ci => ci.CampaignId)
                .IsUnique();

            builder.Entity<CampaignInventoryStock>()
                .HasKey(cis => cis.CampaignInventoryStockId);

            builder.Entity<CampaignInventoryStock>()
                .ToTable(t => t.HasCheckConstraint("CK_CampaignInventoryStocks_CurrentQuantity_NonNegative", "\"CurrentQuantity\" >= 0"));

            builder.Entity<CampaignInventoryStock>()
                .HasOne(cis => cis.CampaignInventory)
                .WithMany(ci => ci.Stocks)
                .HasForeignKey(cis => cis.CampaignInventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CampaignInventoryStock>()
                .HasOne(cis => cis.SupplyItem)
                .WithMany(si => si.CampaignInventoryStocks)
                .HasForeignKey(cis => cis.SupplyItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CampaignInventoryStock>()
                .HasIndex(cis => new { cis.CampaignInventoryId, cis.SupplyItemId })
                .IsUnique();

            builder.Entity<CampaignInventoryTransaction>()
                .HasKey(cit => cit.CampaignInventoryTransactionId);

            builder.Entity<CampaignInventoryTransaction>()
                .Property(cit => cit.TransactionCode)
                .HasMaxLength(30)
                .IsRequired();

            builder.Entity<CampaignInventoryTransaction>()
                .HasIndex(cit => cit.CampaignTeamId);

            builder.Entity<CampaignInventoryTransaction>()
                .HasIndex(cit => cit.DistributionPointId);

            builder.Entity<CampaignInventoryTransaction>()
                .HasIndex(cit => cit.HouseholdDeliveryId);

            builder.Entity<CampaignInventoryTransaction>()
                .HasIndex(cit => cit.ReliefPackageDefinitionId);

            builder.Entity<CampaignInventoryTransaction>()
                .HasOne(cit => cit.CampaignInventory)
                .WithMany(ci => ci.Transactions)
                .HasForeignKey(cit => cit.CampaignInventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CampaignInventoryTransaction>()
                .HasOne(cit => cit.CreatedByUser)
                .WithMany()
                .HasForeignKey(cit => cit.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CampaignInventoryTransaction>()
                .HasOne(cit => cit.SupplyAllocation)
                .WithMany(sa => sa.CampaignInventoryTransactions)
                .HasForeignKey(cit => cit.SupplyAllocationId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<CampaignInventoryTransactionItem>()
                .HasKey(citi => citi.CampaignInventoryTransactionItemId);

            builder.Entity<CampaignInventoryTransactionItem>()
                .HasOne(citi => citi.CampaignInventoryTransaction)
                .WithMany(cit => cit.Items)
                .HasForeignKey(citi => citi.CampaignInventoryTransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CampaignInventoryTransactionItem>()
                .HasOne(citi => citi.SupplyItem)
                .WithMany(si => si.CampaignInventoryTransactionItems)
                .HasForeignKey(citi => citi.SupplyItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // CampaignTask Configuration
            builder.Entity<CampaignTask>()
                .HasKey(ct => ct.CampaignTaskId);

            builder.Entity<CampaignTask>()
                .HasOne(ct => ct.CampaignTeam)
                .WithMany()
                .HasForeignKey(ct => ct.CampaignTeamId)
                .OnDelete(DeleteBehavior.Cascade);

            // CampaignVehicle Configuration
            builder.Entity<CampaignVehicle>()
                .HasKey(cv => cv.CampaignVehicleId);

            builder.Entity<CampaignVehicle>()
                .HasOne(cv => cv.Vehicle)
                .WithMany()
                .HasForeignKey(cv => cv.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CampaignVehicle>()
                .HasOne(cv => cv.Campaign)
                .WithMany()
                .HasForeignKey(cv => cv.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CampaignVehicle>()
                .HasOne(cv => cv.Driver)
                .WithMany()
                .HasForeignKey(cv => cv.AssignedDriverId)
                .OnDelete(DeleteBehavior.SetNull);

            // MemberTask Configuration
            builder.Entity<MemberTask>()
                .HasKey(mt => mt.MemberTaskId);

            builder.Entity<MemberTask>()
                .HasOne(mt => mt.CampaignTask)
                .WithMany(ct => ct.MemberTasks)
                .HasForeignKey(mt => mt.CampaignTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MemberTask>()
                .HasOne(mt => mt.VolunteerProfile)
                .WithMany()
                .HasForeignKey(mt => mt.VolunteerProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // CampaignTaskItem Configuration
            builder.Entity<CampaignTaskItem>()
                .HasKey(cti => cti.CampaignTaskItemId);

            builder.Entity<CampaignTaskItem>()
                .HasOne(cti => cti.CampaignTask)
                .WithMany(ct => ct.CampaignTaskItems)
                .HasForeignKey(cti => cti.CampaignTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CampaignTaskItem>()
                .HasOne(cti => cti.SupplyAllocationItem)
                .WithMany(sai => sai.CampaignTaskItems)
                .HasForeignKey(cti => cti.SupplyAllocationItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // MemberTaskItem Configuration
            builder.Entity<MemberTaskItem>()
                .HasKey(mti => mti.MemberTaskItemId);

            builder.Entity<MemberTaskItem>()
                .HasOne(mti => mti.MemberTask)
                .WithMany(mt => mt.MemberTaskItems)
                .HasForeignKey(mti => mti.MemberTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MemberTaskItem>()
                .HasOne(mti => mti.CampaignTaskItem)
                .WithMany(cti => cti.MemberTaskItems)
                .HasForeignKey(mti => mti.CampaignTaskItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // SupplyAllocation Configuration
            builder.Entity<SupplyAllocation>()
                .HasKey(sa => sa.AllocationId);

            builder.Entity<SupplyAllocation>()
                .HasOne(sa => sa.Campaign)
                .WithMany()
                .HasForeignKey(sa => sa.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SupplyAllocation>()
                .HasOne(sa => sa.SourceInventory)
                .WithMany()
                .HasForeignKey(sa => sa.SourceInventoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupplyAllocation>()
                .HasOne(sa => sa.InventoryTransaction)
                .WithOne(it => it.SupplyAllocation)
                .HasForeignKey<SupplyAllocation>(sa => sa.InventoryTransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            // SupplyAllocationItem Configuration
            builder.Entity<SupplyAllocationItem>()
                .HasKey(sai => sai.AllocationItemId);

            builder.Entity<SupplyAllocationItem>()
                .HasOne(sai => sai.SupplyAllocation)
                .WithMany(sa => sa.Items)
                .HasForeignKey(sai => sai.AllocationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SupplyAllocationItem>()
                .HasOne(sai => sai.SupplyItem)
                .WithMany(s => s.SupplyAllocationItems)
                .HasForeignKey(sai => sai.SupplyItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // CampaignHousehold
            // =========================
            builder.Entity<CampaignHousehold>(entity =>
            {
                entity.HasKey(x => x.CampaignHouseholdId);

                entity.Property(x => x.FulfillmentStatus)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(x => x.DeliveryMode)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(x => x.HouseholdCode)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.HeadOfHouseholdName)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.ContactPhone)
                    .HasMaxLength(50);

                entity.Property(x => x.Address)
                    .HasMaxLength(500);

                entity.HasOne(x => x.Campaign)
                    .WithMany()
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.DistributionPoint)
                    .WithMany(p => p.Households)
                    .HasForeignKey(x => x.DistributionPointId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(x => x.CampaignTeam)
                    .WithMany()
                    .HasForeignKey(x => x.CampaignTeamId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(x => x.Location)
                    .WithMany()
                    .HasForeignKey(x => x.LocationId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(x => new { x.CampaignId, x.HouseholdCode })
                    .IsUnique();
            });

            // =========================
            // DistributionPoint
            // =========================
            builder.Entity<DistributionPoint>(entity =>
            {
                entity.HasKey(x => x.DistributionPointId);

                entity.Property(x => x.Name)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.Address)
                    .HasMaxLength(500);

                entity.Property(x => x.DeliveryMode)
                    .HasConversion<string>()
                    .IsRequired();

                entity.HasOne(x => x.Campaign)
                    .WithMany()
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.ReliefStation)
                    .WithMany()
                    .HasForeignKey(x => x.ReliefStationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CampaignTeam)
                    .WithMany()
                    .HasForeignKey(x => x.CampaignTeamId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(x => x.Location)
                    .WithMany()
                    .HasForeignKey(x => x.LocationId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(x => new { x.CampaignId, x.IsActive });
            });

            // =========================
            // ReliefPackageDefinition
            // =========================
            builder.Entity<ReliefPackageDefinition>(entity =>
            {
                entity.HasKey(x => x.ReliefPackageDefinitionId);

                entity.Property(x => x.Name)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.Description)
                    .HasMaxLength(1000);

                entity.HasOne(x => x.Campaign)
                    .WithMany()
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.OutputSupplyItem)
                    .WithMany(s => s.OutputOfReliefPackageDefinitions)
                    .HasForeignKey(x => x.OutputSupplyItemId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.CampaignId, x.Name });
            });

            // =========================
            // ReliefPackageDefinitionItem
            // =========================
            builder.Entity<ReliefPackageDefinitionItem>(entity =>
            {
                entity.HasKey(x => x.ReliefPackageDefinitionItemId);

                entity.Property(x => x.Unit)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasOne(x => x.ReliefPackageDefinition)
                    .WithMany(p => p.Items)
                    .HasForeignKey(x => x.ReliefPackageDefinitionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.SupplyItem)
                    .WithMany()
                    .HasForeignKey(x => x.SupplyItemId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.ReliefPackageDefinitionId, x.SupplyItemId })
                    .IsUnique();
            });

            // =========================
            // ReliefPackageAssembly
            // =========================
            builder.Entity<ReliefPackageAssembly>(entity =>
            {
                entity.HasKey(x => x.ReliefPackageAssemblyId);

                entity.Property(x => x.Notes)
                    .HasMaxLength(1000);

                entity.HasOne(x => x.Campaign)
                    .WithMany(c => c.ReliefPackageAssemblies)
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ReliefStation)
                    .WithMany(rs => rs.ReliefPackageAssemblies)
                    .HasForeignKey(x => x.ReliefStationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Inventory)
                    .WithMany(i => i.ReliefPackageAssemblies)
                    .HasForeignKey(x => x.InventoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ReliefPackageDefinition)
                    .WithMany(p => p.PackageAssemblies)
                    .HasForeignKey(x => x.ReliefPackageDefinitionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.OutputSupplyItem)
                    .WithMany(s => s.OutputReliefPackageAssemblies)
                    .HasForeignKey(x => x.OutputSupplyItemId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CreatedByUser)
                    .WithMany(u => u.CreatedReliefPackageAssemblies)
                    .HasForeignKey(x => x.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.InventoryId, x.CreatedAt });
                entity.HasIndex(x => new { x.ReliefPackageDefinitionId, x.CreatedAt });
            });

            // =========================
            // ReliefPackageAssemblyDetail
            // =========================
            builder.Entity<ReliefPackageAssemblyDetail>(entity =>
            {
                entity.HasKey(x => x.ReliefPackageAssemblyDetailId);

                entity.Property(x => x.Unit)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasOne(x => x.ReliefPackageAssembly)
                    .WithMany(a => a.Details)
                    .HasForeignKey(x => x.ReliefPackageAssemblyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.SupplyItem)
                    .WithMany(s => s.ReliefPackageAssemblyDetails)
                    .HasForeignKey(x => x.SupplyItemId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.ReliefPackageAssemblyId, x.SupplyItemId })
                    .IsUnique();
            });

            // =========================
            // HouseholdDelivery
            // =========================
            builder.Entity<HouseholdDelivery>(entity =>
            {
                entity.HasKey(x => x.HouseholdDeliveryId);

                entity.Property(x => x.DeliveryMode)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(x => x.Status)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(x => x.Notes)
                    .HasMaxLength(1000);

                entity.HasOne(x => x.Campaign)
                    .WithMany()
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.CampaignHousehold)
                    .WithMany(h => h.Deliveries)
                    .HasForeignKey(x => x.CampaignHouseholdId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.DistributionPoint)
                    .WithMany(p => p.Deliveries)
                    .HasForeignKey(x => x.DistributionPointId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(x => x.CampaignTeam)
                    .WithMany()
                    .HasForeignKey(x => x.CampaignTeamId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(x => x.ReliefPackageDefinition)
                    .WithMany(p => p.HouseholdDeliveries)
                    .HasForeignKey(x => x.ReliefPackageDefinitionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.DeliveredByUser)
                    .WithMany()
                    .HasForeignKey(x => x.DeliveredByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(x => new { x.CampaignId, x.CampaignTeamId, x.Status });
            });

            // =========================
            // HouseholdDeliveryProof
            // =========================
            builder.Entity<HouseholdDeliveryProof>(entity =>
            {
                entity.HasKey(x => x.HouseholdDeliveryProofId);

                entity.Property(x => x.FileUrl)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(x => x.FileType)
                    .HasMaxLength(100);

                entity.Property(x => x.Note)
                    .HasMaxLength(1000);

                entity.HasOne(x => x.HouseholdDelivery)
                    .WithMany(d => d.Proofs)
                    .HasForeignKey(x => x.HouseholdDeliveryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.CapturedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.CapturedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // =========================
            // SupplyShortageRequest
            // =========================
            builder.Entity<SupplyShortageRequest>(entity =>
            {
                entity.HasKey(x => x.SupplyShortageRequestId);

                entity.Property(x => x.Status)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(x => x.Reason)
                    .HasMaxLength(1000);

                entity.Property(x => x.ReviewNote)
                    .HasMaxLength(1000);

                entity.HasOne(x => x.Campaign)
                    .WithMany()
                    .HasForeignKey(x => x.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.DistributionPoint)
                    .WithMany()
                    .HasForeignKey(x => x.DistributionPointId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(x => x.CampaignTeam)
                    .WithMany()
                    .HasForeignKey(x => x.CampaignTeamId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(x => x.RequestedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.RequestedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ReviewedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.ReviewedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(x => new { x.CampaignId, x.Status, x.RequestedAt });
            });

            // =========================
            // SupplyShortageRequestItem
            // =========================
            builder.Entity<SupplyShortageRequestItem>(entity =>
            {
                entity.HasKey(x => x.SupplyShortageRequestItemId);

                entity.Property(x => x.Note)
                    .HasMaxLength(500);

                entity.HasOne(x => x.SupplyShortageRequest)
                    .WithMany(r => r.Items)
                    .HasForeignKey(x => x.SupplyShortageRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.SupplyItem)
                    .WithMany()
                    .HasForeignKey(x => x.SupplyItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // ReliefStation – Location FK

            builder.Entity<ReliefStation>()
                .HasOne(rs => rs.Location)
                .WithMany()
                .HasForeignKey(rs => rs.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // SupplyTransfer Logistics FKs
            builder.Entity<SupplyTransfer>()
                .HasOne(st => st.Vehicle)
                .WithMany(v => v.SupplyTransfers)
                .HasForeignKey(st => st.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SupplyTransfer>()
                .HasOne(st => st.DriverUser)
                .WithMany()
                .HasForeignKey(st => st.DriverUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Campaign – CreatedBy FK
            builder.Entity<Campaign>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // Request (TPT – Table-Per-Type: RescueRequest có bảng riêng,
            // dùng shared PK với bảng Requests)
            // =========================
            builder.Entity<Request>(entity =>
            {
                entity.ToTable("Requests");

                entity.HasKey(r => r.RequestId);

                entity.Property(r => r.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(r => r.CreatedAt)
                    .IsRequired();

                entity.HasOne(r => r.ReporterUser)
                    .WithMany()
                    .HasForeignKey(r => r.ReporterUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(r => r.Location)
                    .WithMany(l => l.Requests)
                    .HasForeignKey(r => r.LocationId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // =========================
            // RescueRequest (TPT)
            // =========================
            builder.Entity<RescueRequest>(entity =>
            {
                entity.ToTable("RescueRequests");

                entity.Property(r => r.RescueRequestStatus)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(r => r.DisasterType)
                    .HasConversion<string>()
                    .IsRequired();

                entity.HasOne(r => r.Campaign)
                    .WithMany(c => c.RescueRequests)
                    .HasForeignKey(r => r.CampaignId)
                    .OnDelete(DeleteBehavior.SetNull);
            });


            // =========================
            // Attachment
            // =========================
            builder.Entity<Attachment>(entity =>
            {
                entity.HasKey(a => a.AttachmentId);

                entity.HasOne(a => a.Request)
                    .WithMany(r => r.Attachments)
                    .HasForeignKey(a => a.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // RequestVerification
            // =========================
            builder.Entity<RequestVerification>(entity =>
            {
                entity.HasKey(rv => rv.RequestVerificationId);

                entity.HasOne(rv => rv.Request)
                    .WithMany(r => r.Verifications)
                    .HasForeignKey(rv => rv.RequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rv => rv.Verifier)
                    .WithMany()
                    .HasForeignKey(rv => rv.VerifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // RescueRequestPriority
            // =========================
            builder.Entity<RescueRequestPriority>(entity =>
            {
                entity.HasKey(rp => new { rp.RescueRequestId, rp.PriorityCriteriaId });

                entity.HasOne(rp => rp.RescueRequest)
                    .WithMany(r => r.RescueRequestPriorities)
                    .HasForeignKey(rp => rp.RescueRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.PriorityCriteria)
                    .WithMany(pc => pc.RescueRequestPriorities)
                    .HasForeignKey(rp => rp.PriorityCriteriaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // RescueOperation
            // =========================
            builder.Entity<RescueOperation>(entity =>
            {
                entity.HasKey(ro => ro.RescueOperationId);

                entity.HasOne(ro => ro.RescueRequest)
                    .WithMany(r => r.RescueOperations)
                    .HasForeignKey(ro => ro.RescueRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ro => ro.Team)
                    .WithMany()
                    .HasForeignKey(ro => ro.TeamId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(ro => ro.Vehicle)
                    .WithMany()
                    .HasForeignKey(ro => ro.VehicleId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(ro => ro.ReliefStation)
                    .WithMany()
                    .HasForeignKey(ro => ro.ReliefStationId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // =========================
            // RescueBatch
            // =========================
            builder.Entity<RescueBatch>(entity =>
            {
                entity.HasKey(rb => rb.RescueBatchId);

                entity.Property(rb => rb.Status)
                    .HasConversion<string>()
                    .IsRequired();

                entity.HasIndex(rb => new { rb.TeamId, rb.IsActive });

                entity.HasOne(rb => rb.Team)
                    .WithMany(t => t.RescueBatches)
                    .HasForeignKey(rb => rb.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // RescueBatchItem
            // =========================
            builder.Entity<RescueBatchItem>(entity =>
            {
                entity.HasKey(rbi => rbi.RescueBatchItemId);

                entity.Property(rbi => rbi.Status)
                    .HasConversion<string>()
                    .IsRequired();

                entity.HasIndex(rbi => new { rbi.RescueBatchId, rbi.SequenceOrder });

                entity.HasOne(rbi => rbi.RescueBatch)
                    .WithMany(rb => rb.Items)
                    .HasForeignKey(rbi => rbi.RescueBatchId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rbi => rbi.RescueRequest)
                    .WithMany(rr => rr.RescueBatchItems)
                    .HasForeignKey(rbi => rbi.RescueRequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // TeamTrackingPoint
            // =========================
            builder.Entity<TeamTrackingPoint>(entity =>
            {
                entity.HasKey(ttp => ttp.TeamTrackingPointId);

                entity.Property(ttp => ttp.Source)
                    .HasConversion<string>()
                    .IsRequired();

                entity.HasOne(ttp => ttp.Team)
                    .WithMany(t => t.TrackingPoints)
                    .HasForeignKey(ttp => ttp.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ttp => ttp.RescueBatch)
                    .WithMany(rb => rb.TrackingPoints)
                    .HasForeignKey(ttp => ttp.RescueBatchId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(ttp => ttp.RescueOperation)
                    .WithMany(ro => ro.TrackingPoints)
                    .HasForeignKey(ttp => ttp.RescueOperationId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(ttp => new { ttp.TeamId, ttp.CapturedAtUtc });
                entity.HasIndex(ttp => new { ttp.RescueOperationId, ttp.CapturedAtUtc });
                entity.HasIndex(ttp => new { ttp.RescueBatchId, ttp.CapturedAtUtc });
            });

            // =========================
            // PriorityCriteria
            // =========================
            builder.Entity<PriorityCriteria>(entity =>
            {
                entity.HasKey(pc => pc.PriorityCriteriaId);

                entity.Property(pc => pc.Name)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(pc => pc.Code)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(pc => pc.Code)
                    .IsUnique();

                entity.Property(pc => pc.DisasterType)
                    .HasConversion<string>()
                    .IsRequired();
            });

            // =========================
            // Donation
            // =========================
            builder.Entity<Donation>(entity =>
            {
                entity.HasKey(d => d.DonationId);

                entity.Property(d => d.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(d => d.Status)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(d => d.DonorName)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(d => d.PayOsPaymentLinkId)
                    .HasMaxLength(128);

                entity.Property(d => d.CheckoutUrl)
                    .HasMaxLength(500);

                entity.HasIndex(d => d.PayOsOrderCode)
                    .IsUnique();

                entity.HasOne(d => d.Campaign)
                    .WithMany(c => c.Donations)
                    .HasForeignKey(d => d.CampaignId)
                    .OnDelete(DeleteBehavior.Cascade);

                // DonorUserId nullable: null nếu donate khi chưa login
                entity.HasOne(d => d.DonorUser)
                    .WithMany()
                    .HasForeignKey(d => d.DonorUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // =========================
            // InKindDonation
            // =========================
            builder.Entity<InKindDonation>(entity =>
            {
                entity.HasKey(ik => ik.InKindDonationId);

                entity.Property(ik => ik.Status)
                    .HasConversion<string>()
                    .IsRequired();

                entity.HasOne(ik => ik.Campaign)
                    .WithMany(c => c.InKindDonations)
                    .HasForeignKey(ik => ik.CampaignId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(ik => ik.ReliefStation)
                    .WithMany(rs => rs.ReceivedInKindDonations)
                    .HasForeignKey(ik => ik.ReliefStationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ik => ik.DonorUser)
                    .WithMany()
                    .HasForeignKey(ik => ik.DonorUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(ik => ik.InventoryTransaction)
                    .WithMany(it => it.InKindDonations)
                    .HasForeignKey(ik => ik.InventoryTransactionId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // =========================
            // InKindDonationDetail
            // =========================
            builder.Entity<InKindDonationDetail>(entity =>
            {
                entity.HasKey(ikd => ikd.InKindDonationDetailId);

                entity.HasOne(ikd => ikd.InKindDonation)
                    .WithMany(ik => ik.InKindDonationDetails)
                    .HasForeignKey(ikd => ikd.InKindDonationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ikd => ikd.SupplyItem)
                    .WithMany(si => si.InKindDonationDetails)
                    .HasForeignKey(ikd => ikd.SupplyItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // DonationTransaction
            // =========================
            builder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasKey(dt => dt.PaymentTransactionId);

                entity.Property(dt => dt.Provider)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(dt => dt.PaymentLinkId)
                    .HasMaxLength(128);

                entity.Property(dt => dt.Reference)
                    .HasMaxLength(100);

                entity.Property(dt => dt.EventCode)
                    .HasMaxLength(20);

                entity.Property(dt => dt.EventDescription)
                    .HasMaxLength(255);

                entity.Property(dt => dt.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(dt => dt.Currency)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(dt => dt.Signature)
                    .HasMaxLength(256)
                    .IsRequired();

                entity.Property(dt => dt.RawPayload)
                    .IsRequired();

                entity.HasIndex(dt => new { dt.Provider, dt.Reference });
                entity.HasIndex(dt => new { dt.Provider, dt.OrderCode, dt.PaymentLinkId });

                entity.HasOne(dt => dt.Donation)
                    .WithMany()
                    .HasForeignKey(dt => dt.DonationId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(dt => dt.User)
                    .WithMany()
                    .HasForeignKey(dt => dt.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // =========================
            // DonationTransactionDetail
            // =========================
            builder.Entity<PaymentTransactionDetail>(entity =>
            {
                entity.HasKey(dtd => dtd.PaymentTransactionDetailId);

                entity.Property(dtd => dtd.FieldName)
                    .HasMaxLength(100);

                entity.Property(dtd => dtd.FieldValue)
                    .HasMaxLength(1000);

                entity.HasOne(dtd => dtd.PaymentTransaction)
                    .WithMany()
                    .HasForeignKey(dtd => dtd.PaymentTransactionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // SupplyTransfer
            // =========================
            builder.Entity<SupplyTransfer>(entity =>
            {
                var evidenceUrlsComparer = new ValueComparer<List<string>>(
                    (left, right) => (left ?? new List<string>()).SequenceEqual(right ?? new List<string>()),
                    list => (list ?? new List<string>()).Aggregate(0, (hash, item) => HashCode.Combine(hash, item == null ? 0 : item.GetHashCode())),
                    list => list == null ? new List<string>() : list.ToList());

                entity.HasKey(st => st.SupplyTransferId);

                entity.Property(st => st.TransferCode)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(st => st.TransferCode)
                    .IsUnique();

                entity.Property(st => st.Status)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(st => st.EvidenceUrls)
                    .HasConversion(
                        value => JsonSerializer.Serialize(value ?? new List<string>(), (JsonSerializerOptions?)null),
                        value => string.IsNullOrWhiteSpace(value)
                            ? new List<string>()
                            : JsonSerializer.Deserialize<List<string>>(value, (JsonSerializerOptions?)null) ?? new List<string>())
                    .Metadata.SetValueComparer(evidenceUrlsComparer);

                // Trạm nguồn
                entity.HasOne(st => st.SourceStation)
                    .WithMany(rs => rs.OutboundTransfers)
                    .HasForeignKey(st => st.SourceStationId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Trạm đích
                entity.HasOne(st => st.DestinationStation)
                    .WithMany(rs => rs.InboundTransfers)
                    .HasForeignKey(st => st.DestinationStationId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Người tạo phiếu
                entity.HasOne(st => st.RequestedByUser)
                    .WithMany()
                    .HasForeignKey(st => st.RequestedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                // Người duyệt (nullable)
                entity.HasOne(st => st.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(st => st.ApprovedBy)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(st => st.Documents)
                    .WithOne(d => d.SupplyTransfer)
                    .HasForeignKey(d => d.SupplyTransferId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // =========================
            // SupplyTransferDocument
            // =========================
            builder.Entity<SupplyTransferDocument>(entity =>
            {
                entity.HasKey(std => std.SupplyTransferDocumentId);

                entity.Property(std => std.DocumentType)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(std => std.FileUrl)
                    .HasMaxLength(2000)
                    .IsRequired();

                entity.Property(std => std.FileName)
                    .HasMaxLength(255);

                entity.Property(std => std.ContentType)
                    .HasMaxLength(100);

                entity.Property(std => std.Notes)
                    .HasMaxLength(1000);

                entity.HasIndex(std => new { std.SupplyTransferId, std.DocumentType, std.Version })
                    .IsUnique();

                entity.HasIndex(std => new { std.SupplyTransferId, std.DocumentType })
                    .HasFilter("\"IsCurrent\" = true")
                    .IsUnique();
            });

            // =========================
            // SupplyTransferItem
            // =========================
            builder.Entity<SupplyTransferItem>(entity =>
            {
                entity.HasKey(sti => sti.SupplyTransferItemId);

                entity.HasOne(sti => sti.SupplyTransfer)
                    .WithMany(st => st.Items)
                    .HasForeignKey(sti => sti.SupplyTransferId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sti => sti.SupplyItem)
                    .WithMany()
                    .HasForeignKey(sti => sti.SupplyItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // Notification
            // =========================
            builder.Entity<Notification>(entity =>
            {
                entity.HasKey(n => n.NotificationId);

                entity.Property(n => n.Type)
                    .HasConversion<string>()
                    .IsRequired();

                entity.Property(n => n.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(n => n.Message)
                    .HasMaxLength(1000);

                entity.Property(n => n.ReferenceType)
                    .HasMaxLength(100);

                entity.Property(n => n.MetadataJson)
                    .HasMaxLength(4000);

                // Index để query nhanh: thông báo chưa đọc của một user
                entity.HasIndex(n => new { n.RecipientId, n.IsRead });

                // Index để sort theo thời gian tạo
                entity.HasIndex(n => n.CreatedAt);

                entity.HasOne(n => n.Recipient)
                    .WithMany()
                    .HasForeignKey(n => n.RecipientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

    }
}
