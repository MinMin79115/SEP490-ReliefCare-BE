using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class PaymentTransactionRepository : GenericRepository<PaymentTransaction>, IPaymentTransactionRepository
    {
        public PaymentTransactionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistsByProviderAndReferenceAsync(string provider, string? reference, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return false;
            }

            return await _context.PaymentTransactions
                .AnyAsync(x => x.Provider == provider && x.Reference == reference, cancellationToken);
        }

        public async Task<List<PaymentTransaction>> GetByDonationIdAsync(Guid donationId, CancellationToken cancellationToken = default)
        {
            return await _context.PaymentTransactions
                .Where(x => x.DonationId == donationId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
