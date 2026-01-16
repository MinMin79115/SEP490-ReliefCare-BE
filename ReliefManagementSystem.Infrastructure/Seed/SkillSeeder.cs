using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Seed
{
    public static class SkillSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Skills.AnyAsync())
                return;

            var skills = new List<Skill>
            {
                new Skill
                {
                    Code = "FIRST_AID",
                    Name = "First Aid",
                    Description = "Basic first aid and emergency response"
                },
                new Skill
                {
                    Code = "SEARCH_RESCUE",
                    Name = "Search and Rescue",
                    Description = "Search, rescue, and evacuation operations"
                },
                new Skill
                {
                    Code = "LOGISTICS",
                    Name = "Logistics Support",
                    Description = "Managing and distributing relief supplies"
                },
                new Skill
                {
                    Code = "MEDICAL_SUPPORT",
                    Name = "Medical Support",
                    Description = "Assisting medical teams during emergencies"
                }
            };

            context.Skills.AddRange(skills);
            await context.SaveChangesAsync();
        }
    }
}
