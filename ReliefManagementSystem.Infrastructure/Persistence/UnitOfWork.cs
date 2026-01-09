using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Infrastructure.Data;
using ReliefManagementSystem.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public IUserRepository Users { get; }
        public IRefreshTokenRepository RefreshTokens { get; }

        // Inventory Management
        public ISupplyItemRepository SupplyItems { get; }
        public IInventoryTransactionRepository InventoryTransactions { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Users = new UserRepository(_context);
            RefreshTokens = new RefreshTokenRepository(_context);
            SupplyItems = new SupplyItemRepository(_context);
            InventoryTransactions = new InventoryTransactionRepository(_context);
        }

        public async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
