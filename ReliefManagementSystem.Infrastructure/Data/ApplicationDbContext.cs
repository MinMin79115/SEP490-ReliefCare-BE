using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public DbSet<VolunteerProfile> VolunteerProfiles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<VolunteerSkill> VolunteerSkills { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<TeamJoinRequest> TeamJoinRequests { get; set; }


        // Vehicle Management
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }

        // Inventory Management
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryStock> InventoryStocks { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<InventoryTransactionItem> InventoryTransactionItems { get; set; }
        public DbSet<SupplyItem> SupplyItems { get; set; }
        public DbSet<ReliefStation> ReliefStations { get; set; }
        public DbSet<ReliefStationTeam> ReliefStationTeams { get; set; }

        // Location
        public DbSet<Location> Locations { get; set; }

        // Campaign Management
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignTeam> CampaignTeams { get; set; }
        public DbSet<CampaignTask> CampaignTasks { get; set; }
        public DbSet<CampaignVehicle> CampaignVehicles { get; set; }
        public DbSet<MemberTask> MemberTasks { get; set; }

        // Supply Allocation
        public DbSet<SupplyAllocation> SupplyAllocations { get; set; }
        public DbSet<SupplyAllocationItem> SupplyAllocationItems { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            //VolunteerProfile configuration
            builder.Entity<VolunteerProfile>()
                .HasOne(v => v.User)
                .WithOne(u => u.VolunteerProfile)
                .HasForeignKey<VolunteerProfile>(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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
                .HasForeignKey(t => t.ModeratorId)
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

            //RefreshToken Configuration
            builder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<VolunteerProfile>()
                .HasKey(v => v.VolunteerProfileId);

            builder.Entity<VolunteerProfile>()
                .HasOne(v => v.User)
                .WithOne(u => u.VolunteerProfile)
                .HasForeignKey<VolunteerProfile>(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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


            builder.Entity<Team>()
                .HasOne(t => t.Leader)
                .WithMany()
                .HasForeignKey(t => t.LeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RefreshToken>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            //Location Configuration

            //builder.Entity<Location>()
            //    .HasKey(l => l.LocationId);

            //builder.Entity<Location>()
            //    .HasOne(l => l.Parent)
            //    .WithMany(l => l.Children)
            //    .HasForeignKey(l => l.ParentId)
            //    .OnDelete(DeleteBehavior.SetNull);

            //builder.Entity<Location>()
            //    .HasIndex(l => l.ParentId);



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
                .HasIndex(vt => vt.TypeName)
                .IsUnique();

            // Inventory Management Configurations
            
            // InventoryStock Configuration - Unique constraint on (InventoryId, SupplyItemId)
            builder.Entity<InventoryStock>()
                .HasKey(i => i.InventoryStockId);

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
                .WithMany()
                .HasForeignKey(i => i.ReliefStationId)
                .OnDelete(DeleteBehavior.Restrict);

            // SupplyItem Configuration
            builder.Entity<SupplyItem>()
                .HasKey(s => s.SupplyItemId);

            // InventoryTransaction Configuration
            builder.Entity<InventoryTransaction>()
                .HasKey(it => it.TransactionId);

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

            // ReliefStation Configuration
            builder.Entity<ReliefStation>()
                .HasKey(rs => rs.ReliefStationId);

            // ReliefStationTeam Configuration (Many-to-Many between ReliefStation and Team)
            builder.Entity<ReliefStationTeam>()
                .HasKey(rst => rst.RelifeStationTeamId);

            builder.Entity<ReliefStationTeam>()
                .HasOne(rst => rst.ReliefStation)
                .WithMany(rs => rs.ReliefStations)
                .HasForeignKey(rst => rst.ReliefStationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ReliefStationTeam>()
                .HasOne(rst => rst.Team)
                .WithMany()
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
                .HasOne(c => c.CreatedByStation)
                .WithMany()
                .HasForeignKey(c => c.CreatedByStationId)
                .OnDelete(DeleteBehavior.Restrict);

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
        }

    }
}
