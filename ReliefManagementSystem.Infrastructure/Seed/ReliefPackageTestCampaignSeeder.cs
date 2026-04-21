using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Seed
{
    public static class ReliefPackageTestCampaignSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            var adminUser = await context.Users
                .FirstOrDefaultAsync(u => u.UserName == "admin")
                ?? throw new Exception("Admin user not found. Run UserSeeder first.");

            var daNang = await context.Locations
                .FirstOrDefaultAsync(l => l.NormalizedName == "da-nang" && l.Level == LocationLevel.Province)
                ?? throw new Exception("Location 'da-nang' (Province) not found. Run LocationExcelSeeder first.");

            var now = DateTime.UtcNow;
            const string campaignName = "Chiến dịch cứu trợ đóng gói - Seed Test";

            var reliefCampaign = await context.Campaigns
                .FirstOrDefaultAsync(c => c.Name == campaignName);

            if (reliefCampaign is null)
            {
                reliefCampaign = new Campaign
                {
                    CampaignId = Guid.NewGuid(),
                    LocationId = daNang.LocationId,
                    CreatedBy = adminUser.Id,
                    Name = campaignName,
                    Description = "Chiến dịch seed phục vụ test luồng tạo gói cứu trợ.",
                    StartDate = now.AddDays(-1),
                    EndDate = now.AddDays(30),
                    Latitude = 16.0544m,
                    Longitude = 108.2022m,
                    AreaRadiusKm = 40,
                    AddressDetail = "Đà Nẵng, Việt Nam",
                    CreatedAt = now,
                    Status = CampaignStatus.Active,
                    Type = CampaignType.Relief,
                    CompletionRule = CampaignCompletionRule.RequiredGoalsMet,
                    AllowOverTarget = true,
                    BudgetTotal = 200000000m,
                    BudgetSpent = 0m
                };

                await context.Campaigns.AddAsync(reliefCampaign);
                await context.SaveChangesAsync();
            }

            var daNangStation = await context.ReliefStations
                .FirstOrDefaultAsync(rs => rs.Name == "Trạm Cứu Trợ Trung Tâm Miền Trung - Đà Nẵng");

            if (daNangStation is null)
            {
                return;
            }

            var attached = await context.CampaignStations
                .FirstOrDefaultAsync(cs => cs.CampaignId == reliefCampaign.CampaignId && cs.ReliefStationId == daNangStation.ReliefStationId);

            if (attached is null)
            {
                await context.CampaignStations.AddAsync(new CampaignStation
                {
                    CampaignId = reliefCampaign.CampaignId,
                    ReliefStationId = daNangStation.ReliefStationId,
                    IsActive = true,
                    AssignedAt = now
                });

                await context.SaveChangesAsync();
            }
            else if (!attached.IsActive)
            {
                attached.IsActive = true;
                attached.AssignedAt = now;
                await context.SaveChangesAsync();
            }
        }
    }
}
