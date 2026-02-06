using ReliefManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IVolunteerProfileRepository
    {
        Task<VolunteerProfile?> GetByUserIdAsync(Guid userId);

        // Include: VolunteerSkills.Skill
        Task<VolunteerProfile?> GetByUserIdWithSkillsAsync(Guid userId);

        Task<List<VolunteerProfile?>> GetAllWithSkillsAsync();

        Task<VolunteerProfile?> GetByIdWithSkillsAndUserAsync(Guid volunteerProfileId);
        Task AddAsync(VolunteerProfile profile);

        Task UpdateAsync(VolunteerProfile profile);

        Task<ApplicationUser?> GetByIdWithVolunteerProfileAsync(Guid userId);
    }
}
