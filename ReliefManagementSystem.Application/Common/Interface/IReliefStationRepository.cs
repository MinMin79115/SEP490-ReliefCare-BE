using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IReliefStationRepository : IGenericRepository<ReliefStation>
    {
        Task<bool> ExistsByNameAsync(string name);

        Task<bool> ExistsProvincialStationInLocationAsync(Guid locationId);
    }
}
