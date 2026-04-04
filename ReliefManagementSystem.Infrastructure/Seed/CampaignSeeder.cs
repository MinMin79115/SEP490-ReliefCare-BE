using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Seed
{
    public static class CampaignSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Campaigns.AnyAsync())
                return;

            var adminUser = await context.Users
                .FirstOrDefaultAsync(u => u.UserName == "admin")
                ?? throw new Exception("Admin user not found. Run UserSeeder first.");

            var daNang = await context.Locations
                .FirstOrDefaultAsync(l => l.NormalizedName == "da-nang" && l.Level == LocationLevel.Province)
                ?? throw new Exception("Location 'da-nang' (Province) not found. Run LocationExcelSeeder first.");

            var now = DateTime.UtcNow;

            var campaign = new Campaign
            {
                CampaignId = Guid.NewGuid(),
                LocationId = daNang.LocationId,
                CreatedBy = adminUser.Id,
                Name = "Chiến dịch cứu hộ miền Trung khẩn cấp",
                Description = "Chiến dịch cứu hộ khẩn cấp cho khu vực miền Trung, ưu tiên cứu nạn và sơ tán.",
                StartDate = now,
                EndDate = now.AddDays(30),
                Latitude = 16.0544m,
                Longitude = 108.2022m,
                AreaRadiusKm = 80,
                AddressDetail = "Đà Nẵng, Việt Nam",
                CreatedAt = now,
                Status = CampaignStatus.Active,
                Type = CampaignType.Rescue,
                CompletionRule = CampaignCompletionRule.RequiredGoalsMet,
                AllowOverTarget = true,
                BudgetTotal = 500000000m,
                BudgetSpent = 0m
            };

            await context.Campaigns.AddAsync(campaign);
            await context.SaveChangesAsync();
        }
    }
}
