using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("UserId", Name = "IX_VolunteerProfiles_UserId", IsUnique = true)]
public partial class VolunteerProfile
{
    [Key]
    public Guid VolunteerProfileId { get; set; }

    public int VerificationStatus { get; set; }

    public Guid? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public Guid UserId { get; set; }

    public string? Descriptions { get; set; }

    public string? Reason { get; set; }

    public int? YearsOfExperience { get; set; }

    [InverseProperty("AssignedDriver")]
    public virtual ICollection<CampaignVehicle> CampaignVehicles { get; set; } = new List<CampaignVehicle>();

    [InverseProperty("VolunteerProfile")]
    public virtual ICollection<MemberTask> MemberTasks { get; set; } = new List<MemberTask>();

    [ForeignKey("UserId")]
    [InverseProperty("VolunteerProfile")]
    public virtual AspNetUser User { get; set; } = null!;

    [InverseProperty("VolunteerProfile")]
    public virtual ICollection<VolunteerCertificate> VolunteerCertificates { get; set; } = new List<VolunteerCertificate>();

    [InverseProperty("VolunteerProfile")]
    public virtual ICollection<VolunteerSkill> VolunteerSkills { get; set; } = new List<VolunteerSkill>();
}
