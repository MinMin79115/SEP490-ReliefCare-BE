using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    /// <summary>
    /// Repository interface for Campaign — basic lookup operations.
    /// Full Campaign CRUD will be implemented in a separate Campaign module.
    /// </summary>
    public interface ICampaignRepository : IGenericRepository<Campaign>
    {
        // ExistsAsync is inherited from IGenericRepository<T>
        // Additional campaign-specific queries will be added in the Campaign module.
    }
}
