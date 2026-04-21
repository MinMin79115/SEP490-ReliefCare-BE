using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Seed
{
    public static class CampaignSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            await Task.CompletedTask;
        }
    }
}
