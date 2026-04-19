using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Donation.DTOs.Request;
using ReliefManagementSystem.Application.Features.Donation.DTOs.Response;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IDonationService
    {
        Task<CreateDonationCheckoutResponse> CreateCheckoutAsync(CreateDonationCheckoutRequest request, CancellationToken cancellationToken = default);
        Task<DonationStatusResponse> GetStatusAsync(Guid donationId, CancellationToken cancellationToken = default);
        Task HandlePayOsWebhookAsync(PayOsWebhookRequest request, CancellationToken cancellationToken = default);
        Task<DonationStatusResponse> ReconcileByOrderCodeAsync(long orderCode, CancellationToken cancellationToken = default);
        Task<Pagination<AdminDonationItemResponse>> GetAdminDonationsAsync(AdminDonationQueryRequest request, CancellationToken cancellationToken = default);
        Task<AdminDonationDetailResponse> GetAdminDonationDetailAsync(Guid donationId, CancellationToken cancellationToken = default);
        Task<DonationStatusResponse> ReconcileAsync(Guid donationId, CancellationToken cancellationToken = default);
        Task<DonationStatusResponse> CancelPendingAsync(Guid donationId, string? reason, CancellationToken cancellationToken = default);
        Task<AdminDonationStatsResponse> GetStatsAsync(AdminDonationQueryRequest? request = null, CancellationToken cancellationToken = default);
        Task<string> ExportCsvAsync(AdminDonationQueryRequest request, CancellationToken cancellationToken = default);
    }
}
