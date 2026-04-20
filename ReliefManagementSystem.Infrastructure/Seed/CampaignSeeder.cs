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

            var rescueCampaign = await context.Campaigns
                .FirstOrDefaultAsync(c => c.Name == "Chiến dịch cứu hộ miền Trung khẩn cấp");

            if (rescueCampaign is null)
            {
                rescueCampaign = new Campaign
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

                await context.Campaigns.AddAsync(rescueCampaign);
            }

            var reliefTeams = await context.Teams
                .Include(t => t.TeamMembers)
                .Where(t => t.TeamType == TeamType.Relief && t.Status == TeamStatus.Active)
                .ToListAsync();

            foreach (var reliefTeam in reliefTeams)
            {
                var existingCampaignTeam = await context.Set<CampaignTeam>()
                    .FirstOrDefaultAsync(ct => ct.CampaignId == rescueCampaign.CampaignId && ct.TeamId == reliefTeam.TeamId);

                if (existingCampaignTeam is not null)
                {
                    continue;
                }

                var role = reliefTeam.Name switch
                {
                    var name when name.Contains("Y Tế", StringComparison.OrdinalIgnoreCase) => CampaignTeamRole.Medical,
                    var name when name.Contains("Logistics", StringComparison.OrdinalIgnoreCase) => CampaignTeamRole.Logistics,
                    _ => CampaignTeamRole.Relief
                };

                await context.Set<CampaignTeam>().AddAsync(new CampaignTeam
                {
                    CampaignTeamId = Guid.NewGuid(),
                    CampaignId = rescueCampaign.CampaignId,
                    TeamId = reliefTeam.TeamId,
                    Role = role,
                    Status = CampaignTeamStatus.Active,
                    AssignedAt = now,
                    IsDelete = false
                });
            }

            var seededTaskTeam = reliefTeams.FirstOrDefault(t => t.Name == "Đội Cứu Hộ Khẩn Cấp ĐN-01");

            if (seededTaskTeam != null)
            {
                var campaignTeam = await context.Set<CampaignTeam>()
                    .FirstOrDefaultAsync(ct => ct.CampaignId == rescueCampaign.CampaignId && ct.TeamId == seededTaskTeam.TeamId);

                if (campaignTeam is null)
                    throw new Exception("Seeded task team must be attached to the rescue campaign before seeding campaign tasks.");

                var seededTask = await context.CampaignTasks
                    .Include(x => x.MemberTasks)
                    .FirstOrDefaultAsync(x => x.CampaignTeamId == campaignTeam.CampaignTeamId && x.Title == "Tuyến giao hàng cô lập Đà Nẵng - Seed");

                if (seededTask is null)
                {
                    seededTask = new CampaignTask
                    {
                        CampaignTaskId = Guid.NewGuid(),
                        CampaignTeamId = campaignTeam.CampaignTeamId,
                        Title = "Tuyến giao hàng cô lập Đà Nẵng - Seed",
                        Description = "Task seed để test relief campaign task API và member assignment trên local.",
                        StartDate = now,
                        DueDate = now.AddDays(2),
                        CreatedBy = adminUser.Id,
                        Status = CampaignTaskStatus.Planned,
                        Priority = TaskPriority.High,
                        CreatedAt = now
                    };

                    await context.CampaignTasks.AddAsync(seededTask);
                }

                var teamMemberUserId = seededTaskTeam.TeamMembers
                    .OrderBy(tm => tm.JoinedAt)
                    .Select(tm => tm.UserId)
                    .FirstOrDefault();

                if (teamMemberUserId != Guid.Empty)
                {
                    var volunteerProfile = await context.VolunteerProfiles
                        .FirstOrDefaultAsync(vp => vp.UserId == teamMemberUserId);

                    if (volunteerProfile != null && !seededTask.MemberTasks.Any(mt => mt.VolunteerProfileId == volunteerProfile.VolunteerProfileId))
                    {
                        seededTask.MemberTasks.Add(new MemberTask
                        {
                            MemberTaskId = Guid.NewGuid(),
                            VolunteerProfileId = volunteerProfile.VolunteerProfileId,
                            SubTaskTitle = "Liên hệ và xác minh tuyến giao - Seed",
                            TaskNote = "Subtask seed để test member task assignment/detail.",
                            AssignedAt = now,
                            Status = MemberTaskStatus.Assigned
                        });
                    }
                }
            }

            var fundraisingCampaign = await context.Campaigns
                .Include(c => c.ResourceGoals)
                .FirstOrDefaultAsync(c => c.Name == "Quỹ hỗ trợ khẩn cấp miền Trung");

            if (fundraisingCampaign is null)
            {
                fundraisingCampaign = new Campaign
                {
                    CampaignId = Guid.NewGuid(),
                    LocationId = daNang.LocationId,
                    CreatedBy = adminUser.Id,
                    Name = "Quỹ hỗ trợ khẩn cấp miền Trung",
                    Description = "Chiến dịch gây quỹ hỗ trợ khẩn cấp cho người dân miền Trung bị ảnh hưởng bởi thiên tai.",
                    StartDate = now.AddDays(-1),
                    EndDate = now.AddDays(45),
                    Latitude = 16.0544m,
                    Longitude = 108.2022m,
                    AreaRadiusKm = 120,
                    AddressDetail = "Đà Nẵng, Việt Nam",
                    CreatedAt = now,
                    Status = CampaignStatus.Active,
                    Type = CampaignType.Fundraising,
                    CompletionRule = CampaignCompletionRule.RequiredGoalsMet,
                    AllowOverTarget = true,
                    BudgetTotal = 300000000m,
                    BudgetSpent = 0m,
                    ResourceGoals = new List<CampaignResourceGoal>
                    {
                        new CampaignResourceGoal
                        {
                            CampaignResourceGoalId = Guid.NewGuid(),
                            ResourceType = CampaignResourceType.Money,
                            TargetAmount = 300000000m,
                            ReceivedAmount = 0m,
                            IsRequired = true,
                            IsMet = false,
                            UpdatedAt = now
                        }
                    }
                };

                await context.Campaigns.AddAsync(fundraisingCampaign);
            }
            else if (!fundraisingCampaign.ResourceGoals.Any(g => g.ResourceType == CampaignResourceType.Money))
            {
                fundraisingCampaign.ResourceGoals.Add(new CampaignResourceGoal
                {
                    CampaignResourceGoalId = Guid.NewGuid(),
                    CampaignId = fundraisingCampaign.CampaignId,
                    ResourceType = CampaignResourceType.Money,
                    TargetAmount = 300000000m,
                    ReceivedAmount = 0m,
                    IsRequired = true,
                    IsMet = false,
                    UpdatedAt = now
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
