using Microsoft.AspNetCore.Identity;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.Inventory;
using ReliefManagementSystem.Application.Services;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Infrastructure.Services
{
    public partial class InventoryService : IInventoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IInventoryItemRepository _inventoryItemRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IWarehouseTransactionRepository _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public InventoryService(
            ICategoryRepository categoryRepository,
            IInventoryItemRepository inventoryItemRepository,
            IBatchRepository batchRepository,
            IWarehouseTransactionRepository transactionRepository,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _categoryRepository = categoryRepository;
            _inventoryItemRepository = inventoryItemRepository;
            _batchRepository = batchRepository;
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        #region Dashboard

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken)
        {
            var totalCategories = (await _categoryRepository.GetAllAsync(cancellationToken)).Count;
            var newItemsToday = await _inventoryItemRepository.CountCreatedTodayAsync(cancellationToken);

            var criticalCount = await _inventoryItemRepository.CountByStatusAsync(InventoryStatus.Critical, cancellationToken);
            var lowCount = await _inventoryItemRepository.CountByStatusAsync(InventoryStatus.Low, cancellationToken);
            var lowStockItems = criticalCount + lowCount;

            var totalExportedToday = await _batchRepository.GetTotalExportedTodayAsync(cancellationToken);

            var items = await _inventoryItemRepository.GetAllAsync(cancellationToken);
            var avgCapacity = items.Any()
                ? items.Average(i => (i.CurrentQuantity / i.MaxCapacity) * 100)
                : 0;

            var criticalItems = await _inventoryItemRepository.GetByStatusAsync(InventoryStatus.Critical, cancellationToken);

            return new DashboardStatsDto
            {
                TotalCategories = totalCategories,
                NewItemsToday = newItemsToday,
                LowStockItems = lowStockItems,
                TotalExportedToday = totalExportedToday,
                AverageCapacityUsage = Math.Round(avgCapacity, 2),
                CriticalItems = criticalItems.Select(MapToDto).ToList()
            };
        }

        #endregion

        #region Inventory Items

        public async Task<List<InventoryItemDto>> GetAllItemsAsync(
            Guid? categoryId,
            CancellationToken cancellationToken)
        {
            var items = categoryId.HasValue
                ? await _inventoryItemRepository.GetByCategoryAsync(categoryId.Value, cancellationToken)
                : await _inventoryItemRepository.GetAllWithCategoryAsync(cancellationToken);

            return items.Select(MapToDto).ToList();
        }

        public async Task<InventoryItemDto?> GetItemByIdAsync(
            Guid itemId,
            CancellationToken cancellationToken)
        {
            var item = await _inventoryItemRepository.GetByIdWithCategoryAsync(itemId, cancellationToken);
            return item == null ? null : MapToDto(item);
        }

        public async Task<InventoryItemDto> CreateItemAsync(
            CreateInventoryItemRequest request,
            CancellationToken cancellationToken)
        {
            // Validate category exists
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category == null)
                throw new Exception($"Category with ID {request.CategoryId} not found");

            // Check code uniqueness
            var codeExists = await _inventoryItemRepository.CodeExistsAsync(request.Code, cancellationToken: cancellationToken);
            if (codeExists)
                throw new Exception($"Item code '{request.Code}' already exists");

            var minThreshold = request.MaxCapacity * 0.15m;
            var status = CalculateStatus(request.InitialQuantity, request.MaxCapacity);

            var item = new InventoryItem
            {
                InventoryItemId = Guid.NewGuid(),
                CategoryId = request.CategoryId,
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                Unit = request.Unit,
                CurrentQuantity = request.InitialQuantity,
                MaxCapacity = request.MaxCapacity,
                MinThreshold = minThreshold,
                Status = status,
                CreatedAt = DateTime.UtcNow
            };

            await _inventoryItemRepository.AddAsync(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await GetItemByIdAsync(item.InventoryItemId, cancellationToken)
                ?? throw new Exception("Failed to retrieve created item");
        }

        public async Task<InventoryItemDto> UpdateItemAsync(
            Guid itemId,
            UpdateInventoryItemRequest request,
            CancellationToken cancellationToken)
        {
            var item = await _inventoryItemRepository.GetByIdAsync(itemId, cancellationToken);
            if (item == null)
                throw new Exception($"Item with ID {itemId} not found");

            item.Name = request.Name;
            item.Description = request.Description;
            item.MaxCapacity = request.MaxCapacity;
            item.MinThreshold = request.MaxCapacity * 0.15m;
            item.Status = CalculateStatus(item.CurrentQuantity, item.MaxCapacity);
            item.UpdatedAt = DateTime.UtcNow;

            _inventoryItemRepository.Update(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await GetItemByIdAsync(itemId, cancellationToken)
                ?? throw new Exception("Failed to retrieve updated item");
        }

        public async Task DeleteItemAsync(Guid itemId, CancellationToken cancellationToken)
        {
            var item = await _inventoryItemRepository.GetByIdAsync(itemId, cancellationToken);
            if (item == null)
                throw new Exception($"Item with ID {itemId} not found");

            // Check if item has transactions
            var transactions = await _transactionRepository.GetByItemIdAsync(itemId, cancellationToken);
            if (transactions.Any())
                throw new Exception("Cannot delete item with existing transactions");

            _inventoryItemRepository.Delete(item);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        #endregion

        #region Categories

        public async Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);
            return categories.Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                Code = c.Code,
                Name = c.Name,
                Description = c.Description
            }).ToList();
        }

        #endregion

        #region Helper Methods

        private static InventoryItemDto MapToDto(InventoryItem item)
        {
            var percentage = (item.CurrentQuantity / item.MaxCapacity) * 100;
            return new InventoryItemDto
            {
                InventoryItemId = item.InventoryItemId,
                CategoryId = item.CategoryId,
                CategoryName = item.Category?.Name ?? "",
                Code = item.Code,
                Name = item.Name,
                Description = item.Description,
                Unit = item.Unit,
                CurrentQuantity = item.CurrentQuantity,
                MaxCapacity = item.MaxCapacity,
                MinThreshold = item.MinThreshold,
                Status = item.Status,
                StatusText = GetStatusText(item.Status),
                PercentageFilled = Math.Round(percentage, 2)
            };
        }

        private static InventoryStatus CalculateStatus(decimal currentQuantity, decimal maxCapacity)
        {
            if (maxCapacity == 0) return InventoryStatus.Critical;

            var percentage = (currentQuantity / maxCapacity) * 100;

            if (currentQuantity >= maxCapacity)
                return InventoryStatus.Full;
            if (percentage >= 50)
                return InventoryStatus.Safe;
            if (percentage >= 15)
                return InventoryStatus.Low;
            return InventoryStatus.Critical;
        }

        private static string GetStatusText(InventoryStatus status)
        {
            return status switch
            {
                InventoryStatus.Critical => "Nguy cấp",
                InventoryStatus.Low => "Cần bổ sung",
                InventoryStatus.Safe => "An toàn",
                InventoryStatus.Full => "Đầy kho",
                _ => "Unknown"
            };
        }

        #endregion
    }
}
