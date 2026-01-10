using Microsoft.Extensions.DependencyInjection;
using ReliefManagementSystem.Application.Features.Auth.Interface;
using ReliefManagementSystem.Application.Features.Auth.Service;
using ReliefManagementSystem.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplication(
           this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();

        
            return services;
        }
    }
}
