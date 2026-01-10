using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IUserRepository
         : IGenericRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetByIdWithVolunteerProfileAsync(
            Guid userId, 
            CancellationToken cancellationToken = default);

        Task<ApplicationUser?> GetByIdWithVolunteerProfileAndSkillsAsync(
            Guid userId, 
            CancellationToken cancellationToken = default);
    }
}
