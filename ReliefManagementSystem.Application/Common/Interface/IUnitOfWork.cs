using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRefreshTokenRepository RefreshTokens { get; }

        // Inventory Management
        ISupplyItemRepository SupplyItems { get; }
        IInventoryTransactionRepository InventoryTransactions { get; }

         Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);

    }
}
