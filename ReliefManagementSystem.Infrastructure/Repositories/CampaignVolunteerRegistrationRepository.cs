using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class CampaignVolunteerRegistrationRepository : ICampaignVolunteerRegistrationRepository
    {
        private readonly ApplicationDbContext _context;

        public CampaignVolunteerRegistrationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CampaignVolunteerRegistration registration, CancellationToken cancellationToken = default)
        {
            await _context.Set<CampaignVolunteerRegistration>().AddAsync(registration, cancellationToken);
        }

        public Task<CampaignVolunteerRegistration?> GetActiveAsync(Guid campaignId, Guid userId, CancellationToken cancellationToken = default)
        {
            return _context.Set<CampaignVolunteerRegistration>()
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x => x.CampaignId == campaignId
                      && x.UserId == userId
                      && x.Status == CampaignVolunteerRegistrationStatus.Registered,
                    cancellationToken);
        }

        public Task<List<CampaignVolunteerRegistration>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return _context.Set<CampaignVolunteerRegistration>()
                .Include(x => x.User)
                .Where(x => x.CampaignId == campaignId)
                .OrderByDescending(x => x.RegisteredAt)
                .ToListAsync(cancellationToken);
        }
    }
}
