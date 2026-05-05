using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Interface
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? Email { get; }
        string? DisplayName { get; }
        IReadOnlyCollection<string> Roles { get; }
        bool IsInRole(string role);
    }
}
