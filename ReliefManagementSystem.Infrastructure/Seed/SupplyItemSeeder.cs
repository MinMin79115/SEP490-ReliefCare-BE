using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Seed
{
    public static class SupplyItemSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            var now = DateTime.UtcNow;

            var seedItems = new List<SupplyItem>
            {
                new SupplyItem
                {
                    SupplyItemId = Guid.NewGuid(),
                    Name = "Gạo tẻ 5kg",
                    Description = "Gạo tẻ đóng gói 5kg cho nhu cầu thiết yếu.",
                    Category = SupplyCategory.LuongThuc,
                    Unit = "bao",
                    EstimatedUnitCost = 85000m,
                    CreatedAt = now
                },
                new SupplyItem
                {
                    SupplyItemId = Guid.NewGuid(),
                    Name = "Mì ăn liền thùng 30 gói",
                    Description = "Thùng mì ăn liền dùng cho cứu trợ khẩn cấp.",
                    Category = SupplyCategory.LuongThuc,
                    Unit = "thùng",
                    EstimatedUnitCost = 120000m,
                    CreatedAt = now
                },
                new SupplyItem
                {
                    SupplyItemId = Guid.NewGuid(),
                    Name = "Nước sạch chai 500ml",
                    Description = "Nước uống đóng chai 500ml.",
                    Category = SupplyCategory.NuocUong,
                    Unit = "chai",
                    EstimatedUnitCost = 5000m,
                    CreatedAt = now
                },
                new SupplyItem
                {
                    SupplyItemId = Guid.NewGuid(),
                    Name = "Chăn mền cứu hộ",
                    Description = "Chăn mền giữ ấm cho hộ dân vùng thiên tai.",
                    Category = SupplyCategory.DungCuVaLeuTrai,
                    Unit = "cái",
                    EstimatedUnitCost = 150000m,
                    CreatedAt = now
                },
                new SupplyItem
                {
                    SupplyItemId = Guid.NewGuid(),
                    Name = "Bộ vệ sinh cá nhân cơ bản",
                    Description = "Bộ gồm xà phòng, bàn chải, kem đánh răng, khăn.",
                    Category = SupplyCategory.Khac,
                    Unit = "bộ",
                    EstimatedUnitCost = 45000m,
                    CreatedAt = now
                },
                new SupplyItem
                {
                    SupplyItemId = Guid.NewGuid(),
                    Name = "Bộ sơ cứu y tế cơ bản",
                    Description = "Bộ sơ cứu gồm băng gạc, sát khuẩn, thuốc thông dụng.",
                    Category = SupplyCategory.YTeVaThuoc,
                    Unit = "bộ",
                    EstimatedUnitCost = 95000m,
                    CreatedAt = now
                }
            };

            var seedNames = seedItems.Select(x => x.Name).ToList();
            var existingNames = await context.SupplyItems
                .Where(x => seedNames.Contains(x.Name))
                .Select(x => x.Name)
                .ToListAsync();

            var missingItems = seedItems
                .Where(x => !existingNames.Contains(x.Name))
                .ToList();

            if (!missingItems.Any())
            {
                return;
            }

            await context.SupplyItems.AddRangeAsync(missingItems);
            await context.SaveChangesAsync();
        }
    }
}
