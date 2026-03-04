using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;

namespace ReliefManagementSystem.Infrastructure.Data;

public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }


    public virtual DbSet<Attachment> Attachments { get; set; }

    public virtual DbSet<Campaign> Campaigns { get; set; }

    public virtual DbSet<CampaignTask> CampaignTasks { get; set; }

    public virtual DbSet<CampaignTeam> CampaignTeams { get; set; }

    public virtual DbSet<CampaignVehicle> CampaignVehicles { get; set; }

    public virtual DbSet<Donation> Donations { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<InventoryStock> InventoryStocks { get; set; }

    public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public virtual DbSet<InventoryTransactionItem> InventoryTransactionItems { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<ManagerProfile> ManagerProfiles { get; set; }

    public virtual DbSet<MemberTask> MemberTasks { get; set; }

    public virtual DbSet<ModeratorProfile> ModeratorProfiles { get; set; }

    public virtual DbSet<PriorityCriteria> PriorityCriterias { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<ReliefNeedItem> ReliefNeedItems { get; set; }

    public virtual DbSet<ReliefRequest> ReliefRequests { get; set; }

    public virtual DbSet<ReliefStation> ReliefStations { get; set; }

    public virtual DbSet<ReliefStationTeam> ReliefStationTeams { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<RequestVerification> RequestVerifications { get; set; }

    public virtual DbSet<RescueOperation> RescueOperations { get; set; }

    public virtual DbSet<RescueRequest> RescueRequests { get; set; }

    public virtual DbSet<RescueRequestPriority> RescueRequestPriorities { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<SupplyAllocation> SupplyAllocations { get; set; }

    public virtual DbSet<SupplyAllocationItem> SupplyAllocationItems { get; set; }

    public virtual DbSet<SupplyItem> SupplyItems { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<TeamJoinRequest> TeamJoinRequests { get; set; }

    public virtual DbSet<TeamMember> TeamMembers { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleType> VehicleTypes { get; set; }

    public virtual DbSet<VolunteerCertificate> VolunteerCertificates { get; set; }

    public virtual DbSet<VolunteerProfile> VolunteerProfiles { get; set; }

    public virtual DbSet<VolunteerSkill> VolunteerSkills { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=relief_db;Username=postgres;Password=12345");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.Property(e => e.AttachmentId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.Property(e => e.CampaignId).ValueGeneratedNever();
            entity.Property(e => e.Name).HasDefaultValueSql("''::text");

            entity.HasOne(d => d.Creator).WithMany().OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.CreatedByStation).WithMany(p => p.Campaigns).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Location).WithMany(p => p.Campaigns).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CampaignTask>(entity =>
        {
            entity.Property(e => e.CampaignTaskId).ValueGeneratedNever();
        });

        modelBuilder.Entity<CampaignTeam>(entity =>
        {
            entity.Property(e => e.CampaignTeamId).ValueGeneratedNever();
        });

        modelBuilder.Entity<CampaignVehicle>(entity =>
        {
            entity.Property(e => e.CampaignVehicleId).ValueGeneratedNever();

            entity.HasOne(d => d.AssignedDriver).WithMany(p => p.CampaignVehicles).OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Vehicle).WithMany(p => p.CampaignVehicles).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Donation>(entity =>
        {
            entity.Property(e => e.DonationId).ValueGeneratedNever();

            entity.HasOne(d => d.DonorUser).WithMany().OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.Property(e => e.InventoryId).ValueGeneratedNever();

            entity.HasOne(d => d.ReliefStation).WithMany(p => p.Inventories).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryStock>(entity =>
        {
            entity.Property(e => e.InventoryStockId).ValueGeneratedNever();
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.Property(e => e.TransactionId).ValueGeneratedNever();

            entity.HasOne(d => d.CreatedByUser).WithMany().OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryTransactionItem>(entity =>
        {
            entity.Property(e => e.TransactionItemId).ValueGeneratedNever();

            entity.HasOne(d => d.SupplyItem).WithMany(p => p.InventoryTransactionItems).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Transaction).WithMany(p => p.Items).HasConstraintName("FK_InventoryTransactionItems_InventoryTransactions_Transaction~");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.Property(e => e.LocationId).ValueGeneratedNever();
            entity.Property(e => e.Path).HasDefaultValueSql("''::text");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ManagerProfile>(entity =>
        {
            entity.Property(e => e.ManagerProfileId).ValueGeneratedNever();

            entity.HasOne(d => d.AssignedLocation).WithMany(p => p.ManagerProfiles).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<MemberTask>(entity =>
        {
            entity.Property(e => e.MemberTaskId).ValueGeneratedNever();
        });

        modelBuilder.Entity<ModeratorProfile>(entity =>
        {
            entity.Property(e => e.ModeratorProfileId).ValueGeneratedNever();
        });

        modelBuilder.Entity<PriorityCriteria>(entity =>
        {
            entity.Property(e => e.PriorityCriteriaId).ValueGeneratedNever();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<ReliefNeedItem>(entity =>
        {
            entity.Property(e => e.ReliefNeedItemId).ValueGeneratedNever();
        });

        modelBuilder.Entity<ReliefRequest>(entity =>
        {
            entity.Property(e => e.RequestId).ValueGeneratedNever();

            entity.HasOne(d => d.Campaign).WithMany(p => p.ReliefRequests).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReliefStation>(entity =>
        {
            entity.Property(e => e.ReliefStationId).ValueGeneratedNever();
            entity.Property(e => e.Level);

            entity.HasOne(d => d.Location).WithMany(p => p.ReliefStations).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.Manager).WithMany().OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.ParentReliefStation).WithMany(p => p.InverseParentReliefStation).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReliefStationTeam>(entity =>
        {
            entity.Property(e => e.ReliefStationTeamId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.Property(e => e.RequestId).ValueGeneratedNever();

            entity.HasOne(d => d.ReporterUser).WithMany().OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RequestVerification>(entity =>
        {
            entity.Property(e => e.RequestVerificationId).ValueGeneratedNever();

            entity.HasOne(d => d.VerifiedByNavigation).WithMany().OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RescueOperation>(entity =>
        {
            entity.Property(e => e.RescueOperationId).ValueGeneratedNever();

            entity.HasOne(d => d.ReliefStation).WithMany(p => p.RescueOperations).OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Team).WithMany(p => p.RescueOperations).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RescueRequest>(entity =>
        {
            entity.Property(e => e.RequestId).ValueGeneratedNever();
        });

        modelBuilder.Entity<RescueRequestPriority>(entity =>
        {
            entity.HasOne(d => d.PriorityCriteria).WithMany(p => p.RescueRequestPriorities).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.Property(e => e.SkillId).ValueGeneratedNever();
        });

        modelBuilder.Entity<SupplyAllocation>(entity =>
        {
            entity.Property(e => e.AllocationId).ValueGeneratedNever();

            entity.HasOne(d => d.SourceInventory).WithMany(p => p.SupplyAllocations).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SupplyAllocationItem>(entity =>
        {
            entity.Property(e => e.AllocationItemId).ValueGeneratedNever();

            entity.HasOne(d => d.SupplyItem).WithMany(p => p.SupplyAllocationItems).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SupplyItem>(entity =>
        {
            entity.Property(e => e.SupplyItemId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.Property(e => e.TeamId).ValueGeneratedNever();

            entity.HasOne(d => d.Leader).WithMany().OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Moderator).WithMany().OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TeamJoinRequest>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.ReviewedByNavigation).WithMany().OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.Property(e => e.VehicleId).ValueGeneratedNever();

            entity.HasOne(d => d.Creator).WithMany().OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.ReliefStation).WithMany(p => p.Vehicles).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.VehicleType).WithMany(p => p.Vehicles).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VehicleType>(entity =>
        {
            entity.Property(e => e.VehicleTypeId).ValueGeneratedNever();
        });

        modelBuilder.Entity<VolunteerCertificate>(entity =>
        {
            entity.Property(e => e.CertificateId).ValueGeneratedNever();
        });

        modelBuilder.Entity<VolunteerProfile>(entity =>
        {
            entity.Property(e => e.VolunteerProfileId).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
