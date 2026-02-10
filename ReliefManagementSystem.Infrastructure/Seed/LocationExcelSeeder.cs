using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Infrastructure.Seed
{
    public class LocationExcelSeeder
    {

        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.Locations.Any())
                return;

            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "DataFiles",
                "FinalData.xlsx"
            );

            using var workbook = new XLWorkbook(filePath);
            var sheet = workbook.Worksheet(1);
            var rows = sheet.RangeUsed().RowsUsed().Skip(1);

            var regionCache = new Dictionary<string, Location>();
            var provinceCache = new Dictionary<string, Location>();

            foreach (var row in rows)
            {
                var regionName = row.Cell(1).GetString().Trim();
                var provinceName = row.Cell(2).GetString().Trim();
                var districtName = row.Cell(3).GetString().Trim();

                var area = row.Cell(4).GetValue<decimal>();
                var population = row.Cell(5).GetValue<long>();


                // 1️⃣ REGION
                if (!regionCache.TryGetValue(regionName, out var region))
                {
                    region = new Location
                    {
                        LocationId = Guid.NewGuid(),
                        Name = regionName,
                        Level = LocationLevel.Region,
                        Status = 1
                    };
                    regionCache[regionName] = region;
                    context.Locations.Add(region);
                }

                // 2️⃣ PROVINCE
                var provinceKey = $"{regionName}-{provinceName}";
                if (!provinceCache.TryGetValue(provinceKey, out var province))
                {
                    province = new Location
                    {
                        LocationId = Guid.NewGuid(),
                        Name = provinceName,
                        ParentId = region.LocationId,
                        Level = LocationLevel.Province,
                        Status = 1
                    };
                    provinceCache[provinceKey] = province;
                    context.Locations.Add(province);
                }

                // 3️⃣ DATA
                if (string.IsNullOrEmpty(districtName))
                {
                    province.Area = area;
                    province.Population = population;
                    province.PopulationDensity = population / area;
                }
                else
                {
                    var district = new Location
                    {
                        LocationId = Guid.NewGuid(),
                        Name = districtName,
                        ParentId = province.LocationId,
                        Area = area,
                        Population = population,
                        PopulationDensity = population / area,
                        Level = LocationLevel.District,
                        Status = 1
                    };
                    context.Locations.Add(district);
                }
            }

            await context.SaveChangesAsync();
        }

    }
}
