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

        public Guid? UserId
        {
            get
            {
                var idString = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return string.IsNullOrEmpty(idString) ? null : Guid.Parse(idString);
            }
        }

        public string? Email =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
    }
}
