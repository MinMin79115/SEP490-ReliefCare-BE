using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Seed
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            await CreateUserAsync(
                userManager,
                context,
                email: "admin@system.com",
                userName: "admin",
                password: "Admin@123",
                role: Role.Admin);

            await CreateUserAsync(
                userManager,
                context,
                email: "user@system.com",
                userName: "user",
                password: "User@123",
                role: Role.User);

            await CreateUserAsync(
                userManager,
                 context,
                email: "moderator@system.com",
                userName: "moderator",
                password: "Moderator@123",
                role: Role.Moderator);

            await CreateUserAsync(
               userManager,
               context,
               email: "user1@system.com",
               userName: "user1",
               password: "User@123",
               role: Role.User);

            await CreateUserAsync(
              userManager,
              context,
              email: "user2@system.com",
              userName: "user2",
              password: "User@123",
              role: Role.User);
        }

        private static async Task CreateUserAsync(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext context,
    string email,
    string userName,
    string password,
    Role role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null) return;

            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = userName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ",
                    result.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(user, role.ToString());

            // ⭐ SEED VOLUNTEER PROFILE
            if (role == Role.Volunteer)
            {
                var exists = await context.VolunteerProfiles
                    .AnyAsync(v => v.UserId == user.Id);

                if (!exists)
                {
                    context.VolunteerProfiles.Add(new VolunteerProfile
                    {
                        UserId = user.Id,
                        VerificationStatus = VerificationStatus.Pending
                    });

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
