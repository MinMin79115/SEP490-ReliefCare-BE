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
            static DateTime UtcDate(int year, int month, int day)
                => new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

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
                managerLevel: LocationLevel.Region);

            await CreateUserAsync(
                userManager,
                context,
                email: "regional.manager1@system.com",
                userName: "regional.manager1",
                password: "Manager@123",
                role: Role.Manager,
                managerLevel: LocationLevel.Region);

            await CreateUserAsync(
                userManager,
                context,
                email: "regional.manager2@system.com",
                userName: "regional.manager2",
                password: "Manager@123",
                role: Role.Manager,
                managerLevel: LocationLevel.Region);

            await CreateUserAsync(
                userManager,
                context,
                email: "regional.manager3@system.com",
                userName: "regional.manager3",
                password: "Manager@123",
                role: Role.Manager,
                managerLevel: LocationLevel.Region);

            // ⭐ ACCOUNT MANAGER - CẤP TỈNH (Province)
            await CreateUserAsync(
                userManager,
                context,
                email: "provincial.manager@system.com",
                userName: "provincial.manager",
                password: "Manager@123",
                role: Role.Manager,
                managerLevel: LocationLevel.Province);


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

            // Volunteer chưa gán team - đầy đủ user + volunteer profile
            var volunteerSeedData = new (string Email, string UserName, string Password, string DisplayName, string PhoneNumber, string Address, string Gender, DateTime DateOfBirth, string Description, int YearsOfExperience, TeamRolePreference PreferredRole, VolunteerType VolunteerType)[]
            {
                ("volunteer.free1@system.com", "volunteer.free1", "Volunteer@123", "Nguyễn Hoàng Anh", "0901000001", "Hải Châu, Đà Nẵng", "Male", UtcDate(1998, 3, 12), "Tình nguyện viên hỗ trợ cứu hộ đường bộ và sơ cấp cứu cơ bản.", 2, TeamRolePreference.Member, VolunteerType.Campaign),
                ("volunteer.free2@system.com", "volunteer.free2", "Volunteer@123", "Trần Minh Châu", "0901000002", "Sơn Trà, Đà Nẵng", "Female", UtcDate(1999, 7, 24), "Có kinh nghiệm hỗ trợ phân luồng và hậu cần tại điểm sơ tán.", 1, TeamRolePreference.Member, VolunteerType.Campaign),
                ("volunteer.free3@system.com", "volunteer.free3", "Volunteer@123", "Lê Quang Huy", "0901000003", "Ngũ Hành Sơn, Đà Nẵng", "Male", UtcDate(1995, 11, 3), "Từng tham gia đội ứng cứu cộng đồng mùa mưa bão.", 3, TeamRolePreference.Driver, VolunteerType.Permanent),
                ("volunteer.free4@system.com", "volunteer.free4", "Volunteer@123", "Phạm Thu Hà", "0901000004", "Liên Chiểu, Đà Nẵng", "Female", UtcDate(1997, 1, 15), "Có kỹ năng điều phối nhu yếu phẩm và hỗ trợ điểm cứu trợ.", 4, TeamRolePreference.Member, VolunteerType.Permanent),
                ("volunteer.free5@system.com", "volunteer.free5", "Volunteer@123", "Đặng Quốc Bảo", "0901000005", "Phú Vang, Huế", "Male", UtcDate(1996, 9, 9), "Tình nguyện viên hỗ trợ vận chuyển người dân trong vùng ngập.", 5, TeamRolePreference.Driver, VolunteerType.Permanent),
                ("volunteer.free6@system.com", "volunteer.free6", "Volunteer@123", "Bùi Ngọc Mai", "0901000006", "TP Huế, Huế", "Female", UtcDate(2000, 4, 18), "Hỗ trợ tiếp nhận, hướng dẫn người dân tại điểm tập kết an toàn.", 1, TeamRolePreference.Member, VolunteerType.Campaign),
                ("volunteer.free7@system.com", "volunteer.free7", "Volunteer@123", "Võ Thành Công", "0901000007", "Hương Trà, Huế", "Male", UtcDate(1994, 12, 1), "Có kinh nghiệm lái xe bán tải, hỗ trợ di chuyển hàng cứu trợ.", 6, TeamRolePreference.Driver, VolunteerType.Permanent),
                ("volunteer.free8@system.com", "volunteer.free8", "Volunteer@123", "Ngô Thị Lan", "0901000008", "Phong Điền, Huế", "Female", UtcDate(1998, 6, 27), "Hỗ trợ sơ cứu và chăm sóc nhóm yếu thế tại hiện trường.", 2, TeamRolePreference.Member, VolunteerType.Campaign),
                ("volunteer.free9@system.com", "volunteer.free9", "Volunteer@123", "Hồ Gia Khánh", "0901000009", "Điện Bàn, Quảng Nam", "Male", UtcDate(1993, 2, 14), "Tình nguyện viên phản ứng nhanh, có thể làm đầu mối nhóm nhỏ.", 7, TeamRolePreference.Leader, VolunteerType.Permanent),
                ("volunteer.free10@system.com", "volunteer.free10", "Volunteer@123", "Phan Mỹ Duyên", "0901000010", "Hòa Vang, Đà Nẵng", "Female", UtcDate(2001, 8, 5), "Tham gia hỗ trợ cứu trợ khẩn cấp và phân phát nhu yếu phẩm.", 1, TeamRolePreference.Member, VolunteerType.Campaign)
            };

            foreach (var volunteer in volunteerSeedData)
            {
                await CreateUserAsync(
                    userManager,
                    context,
                    email: volunteer.Email,
                    userName: volunteer.UserName,
                    password: volunteer.Password,
                    role: Role.Volunteer,
                    displayName: volunteer.DisplayName,
                    phoneNumber: volunteer.PhoneNumber,
                    address: volunteer.Address,
                    gender: volunteer.Gender,
                    dateOfBirth: volunteer.DateOfBirth,
                    volunteerDescription: volunteer.Description,
                    volunteerYearsOfExperience: volunteer.YearsOfExperience,
                    volunteerPreferredRole: volunteer.PreferredRole,
                    volunteerType: volunteer.VolunteerType,
                    volunteerVerificationStatus: VerificationStatus.Approved,
                    volunteerStatus: VolunteerStatus.Active);
            }
        }

        private static async Task CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            string email,
            string userName,
            string password,
            Role role,
            LocationLevel? managerLevel = null,
            string? displayName = null,
            string? phoneNumber = null,
            string? address = null,
            string? gender = null,
            DateTime? dateOfBirth = null,
            string? volunteerDescription = null,
            int? volunteerYearsOfExperience = null,
            TeamRolePreference volunteerPreferredRole = TeamRolePreference.Member,
            VolunteerType volunteerType = VolunteerType.Campaign,
            VerificationStatus volunteerVerificationStatus = VerificationStatus.Pending,
            VolunteerStatus volunteerStatus = VolunteerStatus.Active)
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
                EmailConfirmed = true,
                DisplayName = displayName ?? userName,
                PhoneNumber = phoneNumber,
                Address = address,
                Gender = gender,
                DateOfBirth = dateOfBirth
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
                        VolunteerProfileId = Guid.NewGuid(),
                        UserId = user.Id,
                        VerificationStatus = volunteerVerificationStatus,
                        Status = volunteerStatus,
                        CreatedAt = DateTime.UtcNow,
                        VerifiedAt = volunteerVerificationStatus == VerificationStatus.Approved ? DateTime.UtcNow : null,
                        Descriptions = volunteerDescription,
                        YearsOfExperience = volunteerYearsOfExperience,
                        PreferredTeamRole = volunteerPreferredRole,
                        VolunteerType = volunteerType
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
                        Level = (LocationLevel)managerLevel.Value,
                        AppointedAt = DateTime.UtcNow
                    });

                    await context.SaveChangesAsync();
                }
            }

            // ⭐ SEED MODERATOR PROFILE
            if (role == Role.Moderator)
            {
                var exists = await context.ModeratorProfiles
                    .AnyAsync(mp => mp.UserId == user.Id);

                if (!exists)
                {
                    context.ModeratorProfiles.Add(new ModeratorProfile
                    {
                        UserId = user.Id,
                        AppointedAt = DateTime.UtcNow,
                        IsStationHead = false,
                        Status = ModeratorStatus.Inactive,
                        StatusReason = "Initial state"
                    });

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
