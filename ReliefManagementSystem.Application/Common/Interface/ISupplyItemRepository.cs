namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ISupplyItemRepository : IGenericRepository<Domain.Entities.SupplyItem>
    {
        Task<List<Domain.Entities.SupplyItem>> GetByIdsAsync(
            List<Guid> ids,
            CancellationToken cancellationToken = default);

        Task<Domain.Entities.SupplyItem?> GetByIdWithDetailsAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<List<Domain.Entities.SupplyItem>> GetByCategoryAsync(
            Domain.Enum.SupplyCategory category,
            CancellationToken cancellationToken = default);
    }
}
