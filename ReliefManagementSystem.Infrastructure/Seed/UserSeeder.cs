using Microsoft.AspNetCore.Identity;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
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
            UserManager<ApplicationUser> userManager)
        {
            await CreateUserAsync(
                userManager,
                email: "admin@system.com",
                userName: "admin",
                password: "Admin@123",
                role: Role.Admin);

            await CreateUserAsync(
                userManager,
                email: "user@system.com",
                userName: "user",
                password: "User@123",
                role: Role.User);

            await CreateUserAsync(
                userManager,
                email: "volunteer@system.com",
                userName: "volunteer",
                password: "Volunteer@123",
                role: Role.Volunteer);

            await CreateUserAsync(
                userManager,
                email: "moderator@system.com",
                userName: "moderator",
                password: "Moderator@123",
                role: Role.Moderator);
        }

        private static async Task CreateUserAsync(
            UserManager<ApplicationUser> userManager,
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
        }
    }
}
