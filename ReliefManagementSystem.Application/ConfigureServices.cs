using Microsoft.Extensions.DependencyInjection;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using ReliefManagementSystem.Application.Features.User;

namespace ReliefManagementSystem.Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplication(
           this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<ITeamJoinRequestService, TeamJoinRequestService>();
            services.AddScoped<IStationJoinRequestService, StationJoinRequestService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ISkillService, SkillService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IVehicleTypeService, VehicleTypeService>();
            // Inventory Management
            services.AddScoped<ISupplyItemService, SupplyItemService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();
            services.AddScoped<ISupplyTransferService, SupplyTransferService>();
            // Relief Station Management
            services.AddScoped<IReliefStationService, ReliefStationService>();
            // Supply Allocation
            services.AddScoped<ISupplyAllocationService, SupplyAllocationService>();
            services.AddScoped<IReliefStationService, ReliefStationService>();
            services.AddScoped<ILocationService, LocationService>();

            //Resuce request service

            services.AddScoped<IRescueRequestService, RescueRequestService>();
            services.AddScoped<IDisasterAnalysisService, DisasterAnalysisService>();


            // Priority Criteria CRUD
            services.AddScoped<IPriorityCriteriaService, PriorityCriteriaService>();
            services.AddScoped<IDonationService, DonationService>();
            services.AddScoped<ICampaignService, CampaignService>();
            services.AddScoped<IProcurementService, ProcurementService>();
            services.AddScoped<IFundService, FundService>();
            services.AddScoped<IReliefDistributionService, ReliefDistributionService>();

            services.AddValidatorsFromAssemblyContaining<UpdateUserProfileRequest>();
            return services;
        }
    }
}
