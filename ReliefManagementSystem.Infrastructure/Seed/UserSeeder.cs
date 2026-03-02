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
                email: "moderator@system.com",
                userName: "moderator",
                password: "Moderator@123",
                role: Role.Moderator);
            
            await CreateUserAsync(
                userManager,
                context,
                email: "moderator1@system.com",
                userName: "moderator1",
                password: "Moderator@123",
                role: Role.Moderator);
            
            await CreateUserAsync(
                userManager,
                context,
                email: "moderator2@system.com",
                userName: "moderator2",
                password: "Moderator@123",
                role: Role.Moderator);
            
            await CreateUserAsync(
                userManager,
                context,
                email: "moderator3@system.com",
                userName: "moderator3",
                password: "Moderator@123",
                role: Role.Moderator);

            // ⭐ ACCOUNT MANAGER - CẤP VÙNG (Regional)
            await CreateUserAsync(
                userManager,
                context,
                email: "regional.manager@system.com",
                userName: "regional.manager",
                password: "Manager@123",
                role: Role.Manager,
                managerLevel: ReliefStationLevel.Regional);

            // ⭐ ACCOUNT MANAGER - CẤP TỈNH (Province)
            await CreateUserAsync(
                userManager,
                context,
                email: "provincial.manager@system.com",
                userName: "provincial.manager",
                password: "Manager@123",
                role: Role.Manager,
                managerLevel: ReliefStationLevel.Province);

            // ⭐ ACCOUNT MANAGER - CẤP ĐỊA PHƯƠNG (Local)
            await CreateUserAsync(
                userManager,
                context,
                email: "local.manager@system.com",
                userName: "local.manager",
                password: "Manager@123",
                role: Role.Manager,
                managerLevel: ReliefStationLevel.Local);

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
            
            await CreateUserAsync(
                userManager,
                context,
                email: "user3@system.com",
                userName: "user3",
                password: "User@123",
                role: Role.User);
            
            await CreateUserAsync(
                userManager,
                context,
                email: "user4@system.com",
                userName: "user4",
                password: "User@123",
                role: Role.User);
            
            await CreateUserAsync(
                userManager,
                context,
                email: "user5@system.com",
                userName: "user5",
                password: "User@123",
                role: Role.User);
            
            await CreateUserAsync(
                userManager,
                context,
                email: "user6@system.com",
                userName: "user6",
                password: "User@123",
                role: Role.User);
            
            await CreateUserAsync(
                userManager,
                context,
                email: "user7@system.com",
                userName: "user7",
                password: "User@123",
                role: Role.User);
            
            await CreateUserAsync(
                userManager,
                context,
                email: "user8@system.com",
                userName: "user8",
                password: "User@123",
                role: Role.User);
            
            await CreateUserAsync(
                userManager,
                context,
                email: "user9@system.com",
                userName: "user9",
                password: "User@123",
                role: Role.User);
            
            await CreateUserAsync(
                userManager,
                context,
                email: "user10@system.com",
                userName: "user10",
                password: "User@123",
                role: Role.User);
        }

        private static async Task CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            string email,
            string userName,
            string password,
            Role role,
            ReliefStationLevel? managerLevel = null)
        {
            // ⭐ KIỂM TRA CẢ EMAIL VÀ USERNAME
            var existingUserByEmail = await userManager.FindByEmailAsync(email);
            if (existingUserByEmail != null) return;
    
            var existingUserByName = await userManager.FindByNameAsync(userName);
            if (existingUserByName != null) return;

            var user = new ApplicationUser
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

            // ⭐ SEED MANAGER PROFILE
            if (role == Role.Manager && managerLevel.HasValue)
            {
                var exists = await context.ManagerProfiles
                    .AnyAsync(mp => mp.UserId == user.Id);

                if (!exists)
                {
                    context.ManagerProfiles.Add(new ManagerProfile
                    {
                        UserId = user.Id,
                        Level = managerLevel.Value,
                        AppointedAt = DateTime.UtcNow
                    });

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
