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

        /// <summary>
        /// Trả về UserId từ JWT claim. Nếu claim không tồn tại, trả về <see cref="Guid.Empty"/>.
        /// </summary>
        public Guid UserId
        {
            get
            {
                var value = _httpContextAccessor.HttpContext?
                    .User.FindFirstValue(ClaimTypes.NameIdentifier);

                return Guid.TryParse(value, out var id) ? id : Guid.Empty;
            }
        }

        public string? Email =>
            _httpContextAccessor.HttpContext?
                .User.FindFirstValue(ClaimTypes.Email);
    }
}
