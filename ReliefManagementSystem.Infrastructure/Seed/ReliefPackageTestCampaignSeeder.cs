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

            var existingHouseholdCodes = await context.CampaignHouseholds
                .Where(x => x.CampaignId == reliefCampaign.CampaignId)
                .Select(x => x.HouseholdCode)
                .ToListAsync();

            var codeSet = existingHouseholdCodes
                .Select(x => x.Trim().ToUpperInvariant())
                .ToHashSet();

            var households = new (string Code, string Name, string Phone, string Address, double Lat, double Lng, int Size, bool Isolated, int Flood, int? Isolation, bool Boat, bool Guide, DeliveryMode Mode, string Notes)[]
            {
                ("DN-HH-001", "Nguyễn Văn Bình", "0905000001", "Hải Châu, Đà Nẵng", 16.0601, 108.2201, 4, false, 2, null, false, false, DeliveryMode.PickupAtPoint, "Hộ dân khu vực trung tâm"),
                ("DN-HH-002", "Trần Thị Hoa", "0905000002", "Thanh Khê, Đà Nẵng", 16.0712, 108.1988, 5, false, 3, null, false, false, DeliveryMode.PickupAtPoint, "Có trẻ nhỏ"),
                ("DN-HH-003", "Lê Văn Hùng", "0905000003", "Liên Chiểu, Đà Nẵng", 16.0983, 108.1362, 6, true, 4, 4, true, false, DeliveryMode.DoorToDoor, "Khu vực ngập sâu"),
                ("DN-HH-004", "Phạm Thị Ngọc", "0905000004", "Sơn Trà, Đà Nẵng", 16.0904, 108.2474, 3, false, 2, null, false, false, DeliveryMode.PickupAtPoint, "Cần hỗ trợ lương thực"),
                ("DN-HH-005", "Hoàng Minh Đức", "0905000005", "Ngũ Hành Sơn, Đà Nẵng", 16.0308, 108.2486, 7, true, 5, 5, true, true, DeliveryMode.DoorToDoor, "Có người già và trẻ em"),
                ("DN-HH-006", "Võ Thị Lan", "0905000006", "Cẩm Lệ, Đà Nẵng", 16.0347, 108.2111, 4, false, 3, null, false, false, DeliveryMode.PickupAtPoint, "Thiếu nước sạch"),
                ("DN-HH-007", "Đặng Quốc Nam", "0905000007", "Hòa Vang, Đà Nẵng", 15.9975, 108.1022, 8, true, 4, 4, false, true, DeliveryMode.DoorToDoor, "Đường vào khó tiếp cận"),
                ("DN-HH-008", "Bùi Thị Mai", "0905000008", "Hải Châu, Đà Nẵng", 16.0582, 108.2245, 2, false, 1, null, false, false, DeliveryMode.PickupAtPoint, "Hộ neo đơn"),
                ("DN-HH-009", "Ngô Văn Khoa", "0905000009", "Thanh Khê, Đà Nẵng", 16.0734, 108.1857, 5, false, 2, null, false, false, DeliveryMode.PickupAtPoint, "Cần nhu yếu phẩm cơ bản"),
                ("DN-HH-010", "Lý Thị Hạnh", "0905000010", "Liên Chiểu, Đà Nẵng", 16.1143, 108.1512, 6, true, 4, 3, false, true, DeliveryMode.DoorToDoor, "Khu vực ven sông"),
                ("DN-HH-011", "Phan Văn Tài", "0905000011", "Sơn Trà, Đà Nẵng", 16.1038, 108.2462, 4, false, 2, null, false, false, DeliveryMode.PickupAtPoint, "Ưu tiên gói thực phẩm"),
                ("DN-HH-012", "Trương Thị Thu", "0905000012", "Ngũ Hành Sơn, Đà Nẵng", 16.0256, 108.2523, 3, false, 2, null, false, false, DeliveryMode.PickupAtPoint, "Có người bệnh nền"),
                ("DN-HH-013", "Nguyễn Quốc Khánh", "0905000013", "Cẩm Lệ, Đà Nẵng", 16.0189, 108.2087, 7, true, 5, 5, true, false, DeliveryMode.DoorToDoor, "Mất liên lạc nhiều giờ"),
                ("DN-HH-014", "Đỗ Thị Yến", "0905000014", "Hòa Vang, Đà Nẵng", 15.9884, 108.0918, 5, true, 4, 4, false, true, DeliveryMode.DoorToDoor, "Cần đội địa phương hỗ trợ"),
                ("DN-HH-015", "Lê Văn Phú", "0905000015", "Hải Châu, Đà Nẵng", 16.0554, 108.2144, 4, false, 1, null, false, false, DeliveryMode.PickupAtPoint, "Hỗ trợ gạo và mì"),
                ("DN-HH-016", "Phạm Thị Hương", "0905000016", "Thanh Khê, Đà Nẵng", 16.0688, 108.1934, 6, false, 3, null, false, false, DeliveryMode.PickupAtPoint, "Có trẻ em dưới 5 tuổi"),
                ("DN-HH-017", "Võ Quốc Trung", "0905000017", "Liên Chiểu, Đà Nẵng", 16.1105, 108.1479, 5, true, 4, 3, false, true, DeliveryMode.DoorToDoor, "Khu vực sạt lở nhẹ"),
                ("DN-HH-018", "Bùi Thị Nhung", "0905000018", "Sơn Trà, Đà Nẵng", 16.0992, 108.2489, 2, false, 1, null, false, false, DeliveryMode.PickupAtPoint, "Hộ người cao tuổi"),
                ("DN-HH-019", "Nguyễn Văn Tùng", "0905000019", "Ngũ Hành Sơn, Đà Nẵng", 16.0281, 108.2497, 7, true, 5, 5, true, true, DeliveryMode.DoorToDoor, "Cần nhiều nước uống"),
                ("DN-HH-020", "Trần Mỹ Linh", "0905000020", "Cẩm Lệ, Đà Nẵng", 16.0227, 108.2056, 4, false, 2, null, false, false, DeliveryMode.PickupAtPoint, "Cần gói vệ sinh")
            };

            var newHouseholds = households
                .Where(x => !codeSet.Contains(x.Code.Trim().ToUpperInvariant()))
                .Select(x => new CampaignHousehold
                {
                    CampaignHouseholdId = Guid.NewGuid(),
                    CampaignId = reliefCampaign.CampaignId,
                    HouseholdCode = x.Code,
                    HeadOfHouseholdName = x.Name,
                    ContactPhone = x.Phone,
                    Address = x.Address,
                    Latitude = x.Lat,
                    Longitude = x.Lng,
                    HouseholdSize = x.Size,
                    IsIsolated = x.Isolated,
                    FloodSeverityLevel = x.Flood,
                    IsolationSeverityLevel = x.Isolation,
                    RequiresBoat = x.Boat,
                    RequiresLocalGuide = x.Guide,
                    DeliveryMode = x.Mode,
                    FulfillmentStatus = HouseholdFulfillmentStatus.Pending,
                    Notes = x.Notes,
                    CreatedAt = now
                })
                .ToList();

            if (newHouseholds.Count > 0)
            {
                await context.CampaignHouseholds.AddRangeAsync(newHouseholds);
                await context.SaveChangesAsync();
            }
        }
    }
}
