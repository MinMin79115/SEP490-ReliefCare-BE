using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ILocationRepository : IGenericRepository<Location>
    {
        Task<List<Location>> GetByLevelAsync(LocationLevel level);
        Task<List<Location>> GetChildrenByParentAsync(Guid parentId, LocationLevel level);
        Task<List<Location>> SearchByPathAsync(string path);
        Task<List<Location>> GetAllActiveAsync();
    }
}
