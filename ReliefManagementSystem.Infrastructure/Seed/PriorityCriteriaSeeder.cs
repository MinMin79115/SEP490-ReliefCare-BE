using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Seed
{
    public static class PriorityCriteriaSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {

            if (context.PriorityCriterias.Any())
                return;

            var data = new List<PriorityCriteria>
            {
                //Flood-specific criteria
            new PriorityCriteria
                {
                    Name = "Trapped / Đuối nước",
                    Code = "HUMAN_TRAPPED",
                    Point = 40,
                    DisasterType = DisasterType.Flood,
                    Description = "Người bị mắc kẹt hoặc có nguy cơ đuối nước",
                    Status = "Active"
                },

            new PriorityCriteria
            {
                Name = "Trẻ em / Người già / Phụ nữ mang thai",
                Code = "HUMAN_VULNERABLE",
                Point = 30,
                DisasterType = DisasterType.Flood,
                Description = "Có trẻ em, người già hoặc phụ nữ mang thai",
                Status = "Active"
            },

            new PriorityCriteria
            {
                Name = "Bị thương nặng",
                Code = "HUMAN_SERIOUS_INJURY",
                Point = 25,
                DisasterType = DisasterType.Flood,
                Description = "Có người bị thương nặng cần cấp cứu",
                Status = "Active"
            },


            // ===== 2. ENVIRONMENT & TIME =====

            new PriorityCriteria
            {
                Name = "Nước đang dâng",
                Code = "ENV_WATER_RISING",
                Point = 25,
                DisasterType = DisasterType.Flood,
                Description = "Mực nước tiếp tục dâng",
                Status = "Active"
            },

            new PriorityCriteria
            {
                Name = "Ban đêm / mất điện",
                Code = "ENV_NIGHT_OR_BLACKOUT",
                Point = 15,
                DisasterType = DisasterType.Flood,
                Description = "Thời điểm ban đêm hoặc khu vực mất điện",
                Status = "Active"
            },

            new PriorityCriteria
            {
                Name = "Thời tiết xấu",
                Code = "ENV_BAD_WEATHER",
                Point = 10,
                DisasterType = DisasterType.Flood,
                Description = "Mưa lớn hoặc thời tiết nguy hiểm",
                Status = "Active"
            },


            // ===== 3. SCALE =====

            new PriorityCriteria
            {
                Name = "> 5 người mắc kẹt",
                Code = "SCALE_MORE_THAN_5",
                Point = 20,
                DisasterType = DisasterType.Flood,
                Description = "Có hơn 5 người mắc kẹt",
                Status = "Active"
            },

            new PriorityCriteria
            {
                Name = "2 - 5 người mắc kẹt",
                Code = "SCALE_2_TO_5",
                Point = 10,
                DisasterType = DisasterType.Flood,
                Description = "Có từ 2 đến 5 người mắc kẹt",
                Status = "Active"
            },

            //Landside
            new PriorityCriteria
            {
                Name = "Người bị chôn vùi",
                Code = "LANDSLIDE_BURIED",
                Point = 40,
                DisasterType = DisasterType.Landslide,
                Description = "Người bị đất đá vùi lấp",
                Status = "Active"
            },

            new PriorityCriteria
            {
                Name = "Có trẻ em / người già",
                Code = "LANDSLIDE_VULNERABLE",
                Point = 30,
                DisasterType = DisasterType.Landslide,
                Description = "Có trẻ em hoặc người già mắc kẹt",
                Status = "Active"
            },

            new PriorityCriteria
            {
                Name = "Có người bị thương nặng",
                Code = "LANDSLIDE_SERIOUS_INJURY",
                Point = 25,
                DisasterType = DisasterType.Landslide,
                Description = "Có nạn nhân bị thương nghiêm trọng",
                Status = "Active"
            },

            new PriorityCriteria
            {
                Name = "Nguy cơ sạt lở tiếp",
                Code = "LANDSLIDE_SECONDARY_RISK",
                Point = 25,
                DisasterType = DisasterType.Landslide,
                Description = "Có khả năng xảy ra sạt lở tiếp theo",
                Status = "Active"
            },

            new PriorityCriteria
            {
                Name = "Mưa lớn kéo dài",
                Code = "LANDSLIDE_HEAVY_RAIN",
                Point = 15,
                DisasterType = DisasterType.Landslide,
                Description = "Mưa lớn làm tăng nguy cơ sạt lở",
                Status = "Active"
            },
            new PriorityCriteria
            {
                Name = "Ban đêm / mất điện",
                Code = "LANDSLIDE_NIGHT",
                Point = 10,
                DisasterType = DisasterType.Landslide,
                Description = "Sạt lở xảy ra vào ban đêm hoặc mất điện",
                Status = "Active"
            },

            new PriorityCriteria
            {
                Name = ">5 người mắc kẹt",
                Code = "LANDSLIDE_SCALE_LARGE",
                Point = 20,
                DisasterType = DisasterType.Landslide,
                Description = "Có hơn 5 người mắc kẹt",
                Status = "Active"
            },

            new PriorityCriteria
            {
                PriorityCriteriaId = Guid.Parse("40000000-0000-0000-0000-000000000008"),
                Name = "2–5 người mắc kẹt",
                Code = "LANDSLIDE_SCALE_MEDIUM",
                Point = 10,
                DisasterType = DisasterType.Landslide,
                Description = "Có từ 2 đến 5 người mắc kẹt",
                Status = "Active"
            }
            };


            await context.PriorityCriterias.AddRangeAsync(data);
            await context.SaveChangesAsync();

        }

    }
}
