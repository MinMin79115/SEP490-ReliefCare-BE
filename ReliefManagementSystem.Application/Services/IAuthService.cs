using ReliefManagementSystem.Application.Features.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken);

        Task<AuthResponse> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken);

        Task<AuthResponse> LoginPhoneAsync(
            LoginPhoneRequest request,
            CancellationToken cancellationToken);
    }

}
