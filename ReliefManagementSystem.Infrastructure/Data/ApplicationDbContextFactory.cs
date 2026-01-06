using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ReliefManagementSystem.Infrastructure.Data
{
    public class ApplicationDbContextFactory
        : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                 .UseNpgsql("Host=localhost;Database=relief_db;Username=postgres;Password=12345")
                 .Options;

            return new ApplicationDbContext(options);
        }
    }
}
