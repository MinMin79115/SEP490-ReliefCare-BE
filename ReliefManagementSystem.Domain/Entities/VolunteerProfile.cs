using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities;

[Index("UserId", Name = "IX_VolunteerProfiles_UserId", IsUnique = true)]
public partial class VolunteerProfile
{
    [Key]
    public Guid VolunteerProfileId { get; set; }

    public  VerificationStatus VerificationStatus { get; set; }

    public Guid? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public Guid UserId { get; set; }

    public string? Descriptions { get; set; }

    public string? Reason { get; set; }

    public int? YearsOfExperience { get; set; }

    public virtual ICollection<CampaignVehicle> CampaignVehicles { get; set; } = new List<CampaignVehicle>();

    public virtual ICollection<MemberTask> MemberTasks { get; set; } = new List<MemberTask>();


    public virtual ApplicationUser User { get; set; } = null!;


    public virtual ICollection<VolunteerCertificate> VolunteerCertificates { get; set; } = new List<VolunteerCertificate>();

    public virtual ICollection<VolunteerSkill> VolunteerSkills { get; set; } = new List<VolunteerSkill>();
}
