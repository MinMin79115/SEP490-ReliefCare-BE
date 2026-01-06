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

            builder.Entity<RefreshToken>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);


        }

    }
}
