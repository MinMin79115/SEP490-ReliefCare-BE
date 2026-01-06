using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Services
{
    public interface ITokenService
    {
        Task<TokenResult> GenerateTokenAsync(
            ApplicationUser user,
            string[] scopes,
            CancellationToken cancellationToken);
    }
}
