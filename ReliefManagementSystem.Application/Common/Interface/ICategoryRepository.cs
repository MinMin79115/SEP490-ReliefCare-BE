using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    /// <summary>
    /// Repository for Category entity
    /// </summary>
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<List<Category>> GetAllWithItemsAsync(CancellationToken cancellationToken = default);
    }
}
