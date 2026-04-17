using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Seed
{
    public static class VehicleSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Vehicles.AnyAsync())
            {
                return;
            }

            var stations = await context.ReliefStations
                .AsNoTracking()
                .Select(s => new { s.ReliefStationId })
                .ToListAsync();

            if (!stations.Any())
            {
                throw new Exception("ReliefStations not found. Run ReliefStationSeeder first.");
            }

            var vehicleTypes = await context.VehicleTypes
                .AsNoTracking()
                .Where(vt => !vt.IsDeleted)
                .Select(vt => new { vt.VehicleTypeId })
                .ToListAsync();

            if (!vehicleTypes.Any())
            {
                throw new Exception("VehicleTypes not found. Run VehicleTypeSeeder first.");
            }

            var creatorId = await context.Users
                .AsNoTracking()
                .Where(u => u.UserName == "regional.manager")
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (creatorId == Guid.Empty)
            {
                creatorId = await context.Users
                    .AsNoTracking()
                    .Where(u => u.UserName == "admin")
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();
            }

            if (creatorId == Guid.Empty)
            {
                throw new Exception("Creator user not found. Run UserSeeder first.");
            }

            var now = DateTime.UtcNow;
            var vehicles = new List<Vehicle>();
            var typeIds = vehicleTypes.Select(vt => vt.VehicleTypeId).ToList();

            var plateCounter = 1;
            foreach (var station in stations)
            {
                for (var i = 0; i < 3; i++)
                {
                    vehicles.Add(new Vehicle
                    {
                        VehicleId = Guid.NewGuid(),
                        VehicleTypeId = typeIds[(plateCounter - 1) % typeIds.Count],
                        ReliefStationId = station.ReliefStationId,
                        LicensePlate = $"RC-{plateCounter:00000}",
                        CreatedBy = creatorId,
                        TeamId = null,
                        Status = i == 2 ? VehicleStatus.Busy : VehicleStatus.Free,
                        IsDeleted = false,
                        CreatedAt = now
                    });

                    plateCounter++;
                }
            }

            await context.Vehicles.AddRangeAsync(vehicles);
            await context.SaveChangesAsync();
        }
    }
}
