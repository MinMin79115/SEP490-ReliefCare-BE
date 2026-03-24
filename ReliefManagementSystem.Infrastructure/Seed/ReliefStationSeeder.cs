using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Seed
{
    public static class ReliefStationSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.ReliefStations.Any())
                return;

            // ──────────────────────────────────────────────────
            // Lấy admin Id để dùng làm CreatedBy
            // ──────────────────────────────────────────────────
            var adminUser = await context.Users
                .FirstOrDefaultAsync(u => u.UserName == "admin")
                ?? throw new Exception("Admin user not found. Run UserSeeder first.");

            // ──────────────────────────────────────────────────
            // Helper: lấy LocationId theo tên + cấp
            // ──────────────────────────────────────────────────
            async Task<Guid> GetLocationId(string name, LocationLevel level)
            {
                var loc = await context.Locations
                    .FirstOrDefaultAsync(l => l.NormalizedName == name && l.Level == level)
                    ?? throw new Exception($"Location '{name}' (level={level}) not found. Run LocationExcelSeeder first.");
                return loc.LocationId;
            }

            // Helper: lấy UserId theo email (nullable – manager tuỳ chọn)
            async Task<Guid?> GetManagerId(string email)
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
                return user?.Id;
            }

            // ──────────────────────────────────────────────────
            // 1️⃣  TRẠM CẤP CỨU TRỢ TRUNG TÂM
            //     LocationId = Region (Miền Trung)
            // ──────────────────────────────────────────────────
            var stations = new List<ReliefStation>
            {
                new ReliefStation
                {
                    ReliefStationId    = Guid.NewGuid(),
                    Name               = "Trạm Cấp Phát Trung Tâm Miền Trung",
                    Level              = ReliefStationLevel.Regional,
                    LocationId         = await GetLocationId("phuoc-chanh", LocationLevel.Commune),
                    Address            = "Đà Nẵng, Việt Nam",
                    ContactNumber      = "0236-3823-0002",
                    Longitude          = 108.2208,
                    Latitude           = 16.0544,
                    ReliefStationStatus= ReliefStationStatus.Active,
                    CreatedAt          = DateTime.UtcNow,
                    UpdatedAt          = DateTime.UtcNow
                }
            };


            context.ReliefStations.AddRange(stations);
            await context.SaveChangesAsync();

            var inventories = stations.Select(s => new Inventory
            {
                InventoryId = Guid.NewGuid(),
                ReliefStationId = s.ReliefStationId,
                Level = InventoryLevel.Regional,
                Status = EntityStatus.Active
            }).ToList();

            context.Inventories.AddRange(inventories);

            await context.SaveChangesAsync();
        }
    }
}
