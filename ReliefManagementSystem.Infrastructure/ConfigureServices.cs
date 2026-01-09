using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Services;
using ReliefManagementSystem.Infrastructure.Data;
using ReliefManagementSystem.Infrastructure.Persistence;
using ReliefManagementSystem.Infrastructure.Repositories;
using ReliefManagementSystem.Infrastructure.Security;
using ReliefManagementSystem.Infrastructure.Services;

namespace ReliefManagementSystem.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
             options.UseNpgsql(
         configuration.GetConnectionString("DefaultConnection")));


            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IInventoryService, InventoryService>();
            return services;
        }
    }
}
