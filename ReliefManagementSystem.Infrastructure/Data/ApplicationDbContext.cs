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

        // Inventory Management
        public DbSet<Category> Categories { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<ImportExportBatch> ImportExportBatches { get; set; }
        public DbSet<WarehouseTransaction> WarehouseTransactions { get; set; }

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

            // Inventory Management Configurations
            builder.Entity<Category>()
                .HasIndex(c => c.Code)
                .IsUnique();

            builder.Entity<InventoryItem>()
                .HasOne(i => i.Category)
                .WithMany(c => c.InventoryItems)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryItem>()
                .HasIndex(i => i.Code)
                .IsUnique();

            builder.Entity<InventoryItem>()
                .Property(i => i.CurrentQuantity)
                .HasPrecision(18, 2);

            builder.Entity<InventoryItem>()
                .Property(i => i.MaxCapacity)
                .HasPrecision(18, 2);

            builder.Entity<InventoryItem>()
                .Property(i => i.MinThreshold)
                .HasPrecision(18, 2);

            builder.Entity<ImportExportBatch>()
                .HasOne(b => b.Creator)
                .WithMany()
                .HasForeignKey(b => b.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ImportExportBatch>()
                .HasIndex(b => b.BatchNumber)
                .IsUnique();

            builder.Entity<WarehouseTransaction>()
                .HasOne(t => t.Batch)
                .WithMany(b => b.Transactions)
                .HasForeignKey(t => t.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<WarehouseTransaction>()
                .HasOne(t => t.InventoryItem)
                .WithMany(i => i.Transactions)
                .HasForeignKey(t => t.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WarehouseTransaction>()
                .Property(t => t.Quantity)
                .HasPrecision(18, 2);


        }
    }
}
