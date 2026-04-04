using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IEmailOtpRepository : IGenericRepository<EmailOtp>
    {
        Task<EmailOtp?> GetLatestValidAsync(Guid userId, OtpPurpose purpose, CancellationToken cancellationToken = default);
        Task InvalidateAllActiveAsync(Guid userId, OtpPurpose purpose, CancellationToken cancellationToken = default);
    }
}
