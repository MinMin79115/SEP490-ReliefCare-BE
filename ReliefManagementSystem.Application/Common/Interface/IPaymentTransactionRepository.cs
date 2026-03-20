using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IPaymentTransactionRepository : IGenericRepository<PaymentTransaction>
    {
        Task<bool> ExistsByProviderAndReferenceAsync(string provider, string? reference, CancellationToken cancellationToken = default);
        Task<List<PaymentTransaction>> GetByDonationIdAsync(Guid donationId, CancellationToken cancellationToken = default);
    }
}
