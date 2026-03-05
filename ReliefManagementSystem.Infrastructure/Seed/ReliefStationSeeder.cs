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
            // 1️⃣  TRẠM CẤP VÙNG (Regional) – 4 trạm
            //     LocationId = Region (cấp vùng)
            //     Manager    = regional.manager[0-3]
            // ──────────────────────────────────────────────────
            var stations = new List<ReliefStation>
            {
                new ReliefStation
                {
                    ReliefStationId    = Guid.NewGuid(),
                    Name               = "Trạm Cấp Phát Vùng Bắc Bộ",
                    Level              = ReliefStationLevel.Regional,
                    LocationId         = await GetLocationId("mien-bac", LocationLevel.Region),
                    Address            = "120, Yên Lãng, Hà Nội, Việt Nam",
                    ContactNumber      = "024-3823-0001",
                    Longitude          = 105.8342,
                    Latitude           = 21.0278,
                    Status             = ReliefStationStatus.Active,
                    CreatedAt          = DateTime.UtcNow,
                    UpdatedAt          = DateTime.UtcNow,
                    ParentReliefStationId = null
                },

                new ReliefStation
                {
                    ReliefStationId    = Guid.NewGuid(),
                    Name               = "Trạm Cấp Phát Vùng Trung Bộ",
                    Level              = ReliefStationLevel.Regional,
                    LocationId         = await GetLocationId("mien-trung", LocationLevel.Region),
                    Address            = "Đà Nẵng, Việt Nam",
                    ContactNumber      = "0236-3823-0002",
                    Longitude          = 108.2208,
                    Latitude           = 16.0544,
                    Status             = ReliefStationStatus.Active,
                    CreatedAt          = DateTime.UtcNow,
                    UpdatedAt          = DateTime.UtcNow,
                    ParentReliefStationId = null
                },

                new ReliefStation
                {
                    ReliefStationId    = Guid.NewGuid(),
                    Name               = "Trạm Cấp Phát Vùng Nam Bộ",
                    Level              = ReliefStationLevel.Regional,
                    LocationId         = await GetLocationId("mien-nam", LocationLevel.Region),
                    Address            = "TP. Hồ Chí Minh, Việt Nam",
                    ContactNumber      = "028-3823-0003",
                    Longitude          = 106.6297,
                    Latitude           = 10.8231,
                    Status             = ReliefStationStatus.Active,
                    CreatedAt          = DateTime.UtcNow,
                    UpdatedAt          = DateTime.UtcNow,
                    ParentReliefStationId = null
                },

            };


            context.ReliefStations.AddRange(stations);
            await context.SaveChangesAsync();
        }
    }
}
