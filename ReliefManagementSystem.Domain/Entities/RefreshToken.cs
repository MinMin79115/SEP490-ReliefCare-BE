using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string Token { get; set; } = null!;

        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }

        public DateTime? Revoked { get; set; }

        public string? CreatedByIp { get; set; }
        public string? Device { get; set; }

        public bool IsExpired => DateTime.UtcNow >= Expires;
        public bool IsActive => Revoked == null && !IsExpired;
    }

}
