using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ICampaignVolunteerRegistrationRepository
    {
        Task AddAsync(CampaignVolunteerRegistration registration, CancellationToken cancellationToken = default);
        Task<CampaignVolunteerRegistration?> GetActiveAsync(Guid campaignId, Guid userId, CancellationToken cancellationToken = default);
        Task<List<CampaignVolunteerRegistration>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
    }
}
