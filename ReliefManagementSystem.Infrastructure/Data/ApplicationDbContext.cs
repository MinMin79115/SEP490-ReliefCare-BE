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

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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


        }

    }
}
