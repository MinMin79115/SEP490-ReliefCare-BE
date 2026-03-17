using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Infrastructure.Data;
using ReliefManagementSystem.Infrastructure.Persistence;
using ReliefManagementSystem.Infrastructure.Payments;
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
            services.AddScoped<IImageService, CloudinaryImageService>();
            services.AddScoped<IEmailService, BrevoEmailService>();
            services.AddScoped<INotificationRealtimePublisher, NotificationRealtimePublisher>();
            services.AddSignalR();

            // Team repositories
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
            services.AddScoped<ITeamJoinRequestRepository, TeamJoinRequestRepository>();
            services.AddScoped<IStationJoinRequestRepository, StationJoinRequestRepository>();

            // Volunteer Profile repositories
            services.AddScoped<IVolunteerProfileRepository, VolunteerProfileRepository>();

            //Vehicle Management repositories
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IVehicleTypeRepository, VehicleTypeRepository>();

            // Inventory Management repositories
            services.AddScoped<ISupplyItemRepository, SupplyItemRepository>();
            services.AddScoped<IInventoryRepository, InventoryRepository>();
            services.AddScoped<IInventoryStockRepository, InventoryStockRepository>();
            services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
            // Relief Station repositories
            services.AddScoped<IReliefStationRepository, ReliefStationRepository>();
            services.AddScoped<IReliefStationTeamRepository, ReliefStationTeamRepository>();
            // Supply Allocation repositories
            services.AddScoped<ISupplyAllocationRepository, SupplyAllocationRepository>();
            services.AddScoped<ICampaignRepository, CampaignRepository>();
            services.AddScoped<IDonationRepository, DonationRepository>();
            services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();

            // Relief Station repositories
            services.AddScoped<IReliefStationRepository, ReliefStationRepository>();

            //Location repositories
            services.AddScoped<ILocationRepository, LocationRepository>();


            services.AddScoped<IRescueRequestRepository, RescueRequestRepository>();
            services.AddScoped<IPriorityCriteriaRepository, PriorityCriteriaRepository>();
            services.AddScoped<IRescueRequestPriorityRepository, RescueRequestPriorityRepository>();
            services.AddScoped<IRescueOperationRepository, RescueOperationRepository>();
            services.AddHttpClient<IPayOsGateway, PayOsGateway>();
            return services;

        }
    }
}
