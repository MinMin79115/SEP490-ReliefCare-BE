using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Seed
{
    public static class TeamSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Teams.AnyAsync())
                return;

            var moderator = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "moderator@system.com");
            
            if (moderator == null)
                throw new Exception("Moderator không tồn tại. Phải seed User trước.");

            var user1 = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "user1@system.com");
            
            var user2 = await context.Users
                .FirstOrDefaultAsync(u => u.Email == "user2@system.com");

            var teams = new List<Team>
            {
                new Team
                {
                    TeamId = Guid.NewGuid(),
                    Name = "Đội Cứu Trợ Khẩn Cấp A",
                    Description = "Đội chuyên xử lý các tình huống khẩn cấp và cứu hộ",
                    CreateBy = moderator.Id,
                    LeaderId = user1?.Id, 
                    Status = TeamStatus.Active,
                    CreatedAt = DateTime.UtcNow
                },
                new Team
                {
                    TeamId = Guid.NewGuid(),
                    Name = "Đội Hỗ Trợ Y Tế B",
                    Description = "Đội hỗ trợ y tế và sơ cứu người dân vùng thiên tai",
                    CreateBy = moderator.Id,
                    LeaderId = user2?.Id,
                    Status = TeamStatus.Active,
                    CreatedAt = DateTime.UtcNow
                },
                new Team
                {
                    TeamId = Guid.NewGuid(),
                    Name = "Đội Logistics C",
                    Description = "Đội quản lý và phân phối vật tư cứu trợ",
                    CreateBy = moderator.Id,
                    LeaderId = null, 
                    Status = TeamStatus.Active,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Teams.AddRangeAsync(teams);
            await context.SaveChangesAsync();
        }
    }
}