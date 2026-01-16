using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Infrastructure.Data;
using ReliefManagementSystem.Infrastructure.Persistence;
using ReliefManagementSystem.Infrastructure.Repositories;
using ReliefManagementSystem.Infrastructure.Security;

namespace ReliefManagementSystem.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            //db context
            services.AddDbContext<ApplicationDbContext>(options =>
             options.UseNpgsql(
         configuration.GetConnectionString("DefaultConnection")));

            services.AddHttpContextAccessor();

            //auth service
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IIdentityAuthService, IdentityAuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Team repositories
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
            services.AddScoped<ITeamJoinRequestRepository, TeamJoinRequestRepository>();
            services.AddScoped<IVolunteerProfileRepository, VolunteerProfileRepository>();

            return services;

        }
    }
}
