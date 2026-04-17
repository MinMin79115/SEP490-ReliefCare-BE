using ReliefManagementSystem.Application.Common.Models;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IRealtimeTokenService
    {
        Task<RealtimeTokenResponse> GenerateForCurrentUserAsync(CancellationToken cancellationToken = default);
    }
}
