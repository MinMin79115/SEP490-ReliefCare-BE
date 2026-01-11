using ReliefManagementSystem.Application.Features.Auth.DTOs;
using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.Auth.Interface
{
    public interface IIdentityAuthService
    {
        Task<ApplicationUser> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken);

        Task<ApplicationUser> ValidateByEmailAsync(
            string email,
            string password,
            CancellationToken cancellationToken);

        Task<ApplicationUser> ValidateByPhoneAsync(
            string phone,
            string password,
            CancellationToken cancellationToken);

        Task<ApplicationUser?> ValidateByGoogleAsync(
    CancellationToken cancellationToken);
    }
}
