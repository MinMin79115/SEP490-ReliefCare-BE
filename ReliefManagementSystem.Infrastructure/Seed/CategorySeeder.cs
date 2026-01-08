using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Seed
{
    public static class CategorySeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Categories.AnyAsync())
                return; // Already seeded

            var categories = new List<Category>
            {
                new Category
                {
                    Code = "FOOD",
                    Name = "Lương thực",
                    Description = "Thực phẩm, lương thực khô, nước uống"
                },
                new Category
                {
                    Code = "MEDICAL",
                    Name = "Y tế & Thuốc",
                    Description = "Thuốc men, dụng cụ y tế, băng gạc"
                },
                new Category
                {
                    Code = "WATER",
                    Name = "Nước uống",
                    Description = "Nước đóng chai, nước khoáng"
                },
                new Category
                {
                    Code = "TOOLS",
                    Name = "Dụng cụ & Lều trại",
                    Description = "Dụng cụ cứu hộ, lều bạt, đèn pin"
                },
                new Category
                {
                    Code = "RESCUE",
                    Name = "Cứu hộ",
                    Description = "Áo phao, phao cứu sinh, dây thừng"
                },
                new Category
                {
                    Code = "CLOTHING",
                    Name = "Quần áo",
                    Description = "Quần áo, chăn màn, khăn"
                }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }
    }
}
