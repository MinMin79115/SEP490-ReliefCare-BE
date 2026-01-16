using Microsoft.AspNetCore.Http;
using ReliefManagementSystem.Application.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Security
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId =>
            Guid.Parse(_httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public string? Email =>
            _httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.Email);
    }
}
