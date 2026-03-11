using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.SupplyItem.DTOs.Request;
using ReliefManagementSystem.Application.Features.SupplyItem.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    /// <summary>
    /// Handles business logic for supply item master data management.
    /// </summary>
    public class SupplyItemService : ISupplyItemService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SupplyItemService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public async Task<SupplyItemResponse> CreateSupplyItemAsync(
            CreateSupplyItemRequest request,
            CancellationToken cancellationToken = default)
        {
            if (await _unitOfWork.SupplyItems.IsNameExistsAsync(request.Name, cancellationToken: cancellationToken))
            {
                throw new InvalidOperationException($"Supply item with name '{request.Name}' already exists.");
            }

            var supplyItem = new Domain.Entities.SupplyItem
            {
                SupplyItemId = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IconUrl = request.IconUrl,
                Category = request.Category,
                Unit = request.Unit,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.SupplyItems.AddAsync(supplyItem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(supplyItem);
        }

        /// <inheritdoc/>
        public async Task<SupplyItemResponse> GetSupplyItemByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var supplyItem = await _unitOfWork.SupplyItems.GetByIdAsync(id);
            if (supplyItem is null)
            {
                throw new KeyNotFoundException($"Supply item with id '{id}' was not found.");
            }

            return MapToResponse(supplyItem);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<SupplyItemResponse>> GetAllSupplyItemsAsync(
            SupplyCategory? category = null,
            CancellationToken cancellationToken = default)
        {
            var items = await _unitOfWork.SupplyItems.GetAllAsync(category, cancellationToken);
            return items.Select(MapToResponse).ToList();
        }

        /// <inheritdoc/>
        public async Task<SupplyItemResponse> UpdateSupplyItemAsync(
            Guid id,
            UpdateSupplyItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var supplyItem = await _unitOfWork.SupplyItems.GetByIdAsync(id);
            if (supplyItem is null)
            {
                throw new KeyNotFoundException($"Supply item with id '{id}' was not found.");
            }

            if (await _unitOfWork.SupplyItems.IsNameExistsAsync(request.Name, excludeId: id, cancellationToken: cancellationToken))
            {
                throw new InvalidOperationException($"Supply item with name '{request.Name}' already exists.");
            }

            supplyItem.Name = request.Name;
            supplyItem.Description = request.Description;
            supplyItem.IconUrl = request.IconUrl;
            supplyItem.Category = request.Category;
            supplyItem.Unit = request.Unit;
            supplyItem.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SupplyItems.UpdateAsync(supplyItem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(supplyItem);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteSupplyItemAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var supplyItem = await _unitOfWork.SupplyItems.GetByIdAsync(id);
            if (supplyItem is null)
            {
                throw new KeyNotFoundException($"Supply item with id '{id}' was not found.");
            }

            await _unitOfWork.SupplyItems.DeleteAsync(supplyItem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        // ─── Private Helpers ───────────────────────────────────────────────────

        private static SupplyItemResponse MapToResponse(Domain.Entities.SupplyItem item) => new()
        {
            SupplyItemId = item.SupplyItemId,
            Name = item.Name,
            Description = item.Description,
            IconUrl = item.IconUrl,
            Category = item.Category,
            Unit = item.Unit,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }
}
