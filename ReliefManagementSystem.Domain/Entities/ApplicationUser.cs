using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReliefManagementSystem.Domain.Entities;

public partial class ApplicationUser : IdentityUser<Guid>
{
    public string? PictureUrl { get; set; }

    public string? PicturePublicId { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? DisplayName { get; set; }

    public string? Address { get; set; }

    public ManagerProfile? ManagerProfile { get; set; }
    public ModeratorProfile? ModeratorProfile { get; set; }
    public VolunteerProfile VolunteerProfile { get; set; }
    public ICollection<TeamMember> TeamMembers { get; set; }
    public ReliefStation? ManagedStation { get; set; }

    
}
