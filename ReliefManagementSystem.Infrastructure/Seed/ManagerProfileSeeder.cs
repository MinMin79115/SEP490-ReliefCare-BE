using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Seed
{
    /// <summary>
    /// Gán ReliefStationId và AssignedLocationId cho ManagerProfile của các regional managers.
    /// Phải chạy sau ReliefStationSeeder.
    /// </summary>
    public static class ManagerProfileSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // ──────────────────────────────────────────────────
            // Lấy tất cả trạm cấp vùng đã seed
            // ──────────────────────────────────────────────────
            var regionalStations = await context.ReliefStations
                .Where(rs => rs.Level == ReliefStationLevel.Regional)
                .ToListAsync();

            if (!regionalStations.Any())
                throw new Exception("No regional relief stations found. Run ReliefStationSeeder first.");

            // ──────────────────────────────────────────────────
            // Mapping: email manager → trạm cấp vùng tương ứng
            // (Thứ tự trùng với ReliefStationSeeder)
            // ──────────────────────────────────────────────────
            var mapping = new[]
            {
                new { Email = "regional.manager@system.com", ContactNumber = "0236-3823-0002"},
            };

            foreach (var entry in mapping)
            {
                // Lấy user
                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Email == entry.Email);
                if (user == null) continue;

                // Lấy ManagerProfile đã tồn tại (tạo bởi UserSeeder)
                var profile = await context.ManagerProfiles
                    .FirstOrDefaultAsync(mp => mp.UserId == user.Id);
                if (profile == null) continue;


                // Lấy trạm tương ứng
                var station = regionalStations
                    .FirstOrDefault(rs => rs.ContactNumber == entry.ContactNumber);
                if (station == null) continue;

                // Gán ReliefStationId và AssignedLocationId cho profile
                profile.AssignedLocationId = station.LocationId;
            }

            await context.SaveChangesAsync();
        }
    }
}
