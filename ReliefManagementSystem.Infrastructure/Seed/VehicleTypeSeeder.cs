using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Seed
{
    public static class VehicleTypeSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.VehicleTypes.AnyAsync())
            {
                return;
            }

            var now = DateTime.UtcNow;
            var vehicleTypes = new List<VehicleType>
            {
                new VehicleType
                {
                    VehicleTypeId = Guid.NewGuid(),
                    TypeName = "Xe Tải Nhẹ",
                    DefaultCapacity = 1500,
                    CapacityKind = CapacityKind.CargoWeight,
                    CapacityUnit = "kg",
                    Description = "Vận chuyển nhu yếu phẩm, nước sạch, đồ dùng khẩn cấp",
                    CreatedAt = now,
                    IsDeleted = false
                },
                new VehicleType
                {
                    VehicleTypeId = Guid.NewGuid(),
                    TypeName = "Xe Tải Trung",
                    DefaultCapacity = 3500,
                    CapacityKind = CapacityKind.CargoWeight,
                    CapacityUnit = "kg",
                    Description = "Vận chuyển hàng cứu trợ liên tỉnh",
                    CreatedAt = now,
                    IsDeleted = false
                },
                new VehicleType
                {
                    VehicleTypeId = Guid.NewGuid(),
                    TypeName = "Xe Bán Tải",
                    DefaultCapacity = 900,
                    CapacityKind = CapacityKind.CargoWeight,
                    CapacityUnit = "kg",
                    Description = "Linh hoạt đi vào đường nhỏ và khu vực bị ngập nhẹ",
                    CreatedAt = now,
                    IsDeleted = false
                },
                new VehicleType
                {
                    VehicleTypeId = Guid.NewGuid(),
                    TypeName = "Xe Cứu Thương",
                    DefaultCapacity = 4,
                    CapacityKind = CapacityKind.PassengerCount,
                    CapacityUnit = "people",
                    Description = "Vận chuyển người bị thương và hỗ trợ sơ cứu",
                    CreatedAt = now,
                    IsDeleted = false
                },
                new VehicleType
                {
                    VehicleTypeId = Guid.NewGuid(),
                    TypeName = "Xe Bán Chuyên Dụng",
                    DefaultCapacity = 2000,
                    CapacityKind = CapacityKind.CargoWeight,
                    CapacityUnit = "kg",
                    Description = "Phù hợp địa hình khó, bố trí đội cứu hộ cơ động",
                    CreatedAt = now,
                    IsDeleted = false
                },
                new VehicleType
                {
                    VehicleTypeId = Guid.NewGuid(),
                    TypeName = "Xe Khách 16 Chỗ",
                    DefaultCapacity = 16,
                    CapacityKind = CapacityKind.PassengerCount,
                    CapacityUnit = "people",
                    Description = "Chở tình nguyện viên và đội phản ứng nhanh",
                    CreatedAt = now,
                    IsDeleted = false
                }
            };

            await context.VehicleTypes.AddRangeAsync(vehicleTypes);
            await context.SaveChangesAsync();
        }
    }
}
