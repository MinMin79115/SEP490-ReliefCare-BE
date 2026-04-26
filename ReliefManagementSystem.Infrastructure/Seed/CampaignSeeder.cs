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
            var adminUser = await context.Users
                .FirstOrDefaultAsync(u => u.UserName == "admin")
                ?? throw new Exception("Admin user not found. Run UserSeeder first.");

            var daNang = await context.Locations
                .FirstOrDefaultAsync(l => l.NormalizedName == "da-nang" && l.Level == LocationLevel.Province)
                ?? throw new Exception("Location 'da-nang' (Province) not found. Run LocationExcelSeeder first.");

            var now = DateTime.UtcNow;
            const string campaignName = "Chiến dịch gây quỹ khẩn cấp miền Trung - Donation Test";

            var fundraisingCampaign = await context.Campaigns
                .Include(c => c.ResourceGoals)
                .FirstOrDefaultAsync(c => c.Name == campaignName);

            if (fundraisingCampaign is null)
            {
                fundraisingCampaign = new Campaign
                {
                    CampaignId = Guid.NewGuid(),
                    LocationId = daNang.LocationId,
                    CreatedBy = adminUser.Id,
                    Name = campaignName,
                    Description = "Campaign seed phục vụ test chuẩn luồng donation PayOS.",
                    StartDate = now.AddDays(-1),
                    EndDate = now.AddDays(60),
                    Latitude = 16.0544m,
                    Longitude = 108.2022m,
                    AreaRadiusKm = 50,
                    AddressDetail = "Đà Nẵng, Việt Nam",
                    CreatedAt = now,
                    Status = CampaignStatus.Active,
                    Type = CampaignType.Fundraising,
                    CompletionRule = CampaignCompletionRule.RequiredGoalsMet,
                    AllowOverTarget = true,
                    BudgetTotal = 0m,
                    BudgetSpent = 0m
                };

                fundraisingCampaign.ResourceGoals.Add(new CampaignResourceGoal
                {
                    CampaignResourceGoalId = Guid.NewGuid(),
                    CampaignId = fundraisingCampaign.CampaignId,
                    ResourceType = CampaignResourceType.Money,
                    TargetAmount = 50000000m,
                    ReceivedAmount = 0m,
                    IsRequired = true,
                    IsMet = false,
                    UpdatedAt = now
                });

                await context.Campaigns.AddAsync(fundraisingCampaign);
                await context.SaveChangesAsync();
                return;
            }

            fundraisingCampaign.LocationId = daNang.LocationId;
            fundraisingCampaign.CreatedBy = adminUser.Id;
            fundraisingCampaign.Description ??= "Campaign seed phục vụ test chuẩn luồng donation PayOS.";
            fundraisingCampaign.StartDate = fundraisingCampaign.StartDate > now ? now.AddDays(-1) : fundraisingCampaign.StartDate;
            fundraisingCampaign.EndDate = fundraisingCampaign.EndDate < now ? now.AddDays(60) : fundraisingCampaign.EndDate;
            fundraisingCampaign.Latitude = fundraisingCampaign.Latitude == 0 ? 16.0544m : fundraisingCampaign.Latitude;
            fundraisingCampaign.Longitude = fundraisingCampaign.Longitude == 0 ? 108.2022m : fundraisingCampaign.Longitude;
            fundraisingCampaign.AreaRadiusKm = fundraisingCampaign.AreaRadiusKm <= 0 ? 50 : fundraisingCampaign.AreaRadiusKm;
            fundraisingCampaign.AddressDetail ??= "Đà Nẵng, Việt Nam";
            fundraisingCampaign.Status = CampaignStatus.Active;
            fundraisingCampaign.Type = CampaignType.Fundraising;
            fundraisingCampaign.CompletionRule = CampaignCompletionRule.RequiredGoalsMet;
            fundraisingCampaign.AllowOverTarget = true;
            if (fundraisingCampaign.BudgetSpent < 0)
            {
                fundraisingCampaign.BudgetSpent = 0m;
            }

            var moneyGoal = fundraisingCampaign.ResourceGoals
                .FirstOrDefault(g => g.ResourceType == CampaignResourceType.Money);

            if (moneyGoal is null)
            {
                fundraisingCampaign.ResourceGoals.Add(new CampaignResourceGoal
                {
                    CampaignResourceGoalId = Guid.NewGuid(),
                    CampaignId = fundraisingCampaign.CampaignId,
                    ResourceType = CampaignResourceType.Money,
                    TargetAmount = 50000000m,
                    ReceivedAmount = 0m,
                    IsRequired = true,
                    IsMet = false,
                    UpdatedAt = now
                });
            }
            else
            {
                if (moneyGoal.TargetAmount <= 0)
                {
                    moneyGoal.TargetAmount = 50000000m;
                }

                if (moneyGoal.ReceivedAmount < 0)
                {
                    moneyGoal.ReceivedAmount = 0m;
                }

                moneyGoal.IsRequired = true;
                moneyGoal.UpdatedAt = now;
            }

            await context.SaveChangesAsync();
        }
    }
}
