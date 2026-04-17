using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Seed
{
    public static class ReliefStationSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.ReliefStations.Any())
                return;

            var now = DateTime.UtcNow;
            var passwordHasher = new PasswordHasher<ApplicationUser>();

            // ──────────────────────────────────────────────────
            // Helper: lấy LocationId theo tên + cấp
            // ──────────────────────────────────────────────────
            async Task<Guid> GetLocationId(string name, LocationLevel level)
            {
                var loc = await context.Locations
                    .FirstOrDefaultAsync(l => l.NormalizedName == name && l.Level == level)
                    ?? throw new Exception($"Location '{name}' (level={level}) not found. Run LocationExcelSeeder first.");
                return loc.LocationId;
            }

            // Tạo user điều phối để làm CreateBy cho team
            ApplicationUser BuildUser(string userName, string email, string displayName, string phone, string address)
            {
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = userName,
                    NormalizedUserName = userName.ToUpperInvariant(),
                    Email = email,
                    NormalizedEmail = email.ToUpperInvariant(),
                    EmailConfirmed = true,
                    PhoneNumber = phone,
                    PhoneNumberConfirmed = true,
                    DisplayName = displayName,
                    Address = address,
                    Gender = "Other",
                    DateOfBirth = new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                user.PasswordHash = passwordHasher.HashPassword(user, "Abc@123456");
                return user;
            }

            var stationCoordinatorDn = BuildUser(
                "mod.danang.seed",
                "mod.danang.seed@relief.local",
                "Điều phối Đà Nẵng",
                "0901000001",
                "Đà Nẵng, Việt Nam");

            var stationCoordinatorHue = BuildUser(
                "mod.hue.seed",
                "mod.hue.seed@relief.local",
                "Điều phối Huế",
                "0901000002",
                "Huế, Việt Nam");

            var volunteers = new List<ApplicationUser>
            {
                BuildUser("v.dn.1", "v.dn.1@relief.local", "Nguyễn Văn Minh", "0902000001", "Đà Nẵng"),
                BuildUser("v.dn.2", "v.dn.2@relief.local", "Trần Đức Anh", "0902000002", "Đà Nẵng"),
                BuildUser("v.dn.3", "v.dn.3@relief.local", "Phạm Hoàng Long", "0902000003", "Đà Nẵng"),
                BuildUser("v.dn.4", "v.dn.4@relief.local", "Lê Tuấn Kiệt", "0902000004", "Đà Nẵng"),
                BuildUser("v.hue.1", "v.hue.1@relief.local", "Ngô Quang Huy", "0903000001", "Huế"),
                BuildUser("v.hue.2", "v.hue.2@relief.local", "Võ Hải Nam", "0903000002", "Huế"),
                BuildUser("v.hue.3", "v.hue.3@relief.local", "Bùi Anh Tuấn", "0903000003", "Huế"),
                BuildUser("v.hue.4", "v.hue.4@relief.local", "Đặng Minh Quân", "0903000004", "Huế")
            };

            var allUsers = new List<ApplicationUser> { stationCoordinatorDn, stationCoordinatorHue };
            allUsers.AddRange(volunteers);
            context.Users.AddRange(allUsers);
            await context.SaveChangesAsync();

            var volunteerRoleId = await context.Roles
                .Where(r => r.Name == Role.Volunteer.ToString())
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (volunteerRoleId == Guid.Empty)
            {
                throw new Exception("Role 'Volunteer' not found. Run RoleSeeder first.");
            }

            var volunteerUserRoles = volunteers.Select(v => new IdentityUserRole<Guid>
            {
                UserId = v.Id,
                RoleId = volunteerRoleId
            }).ToList();

            context.UserRoles.AddRange(volunteerUserRoles);
            await context.SaveChangesAsync();

            // Tạo VolunteerProfile đầy đủ cho 8 volunteer
            var volunteerProfiles = volunteers.Select(v => new VolunteerProfile
            {
                VolunteerProfileId = Guid.NewGuid(),
                UserId = v.Id,
                VerificationStatus = VerificationStatus.Approved,
                Status = VolunteerStatus.Active,
                VerifiedAt = now,
                CreatedAt = now,
                Descriptions = "Tình nguyện viên cứu hộ khẩn cấp",
                YearsOfExperience = 2,
                PreferredTeamRole = TeamRolePreference.Member,
                VolunteerType = VolunteerType.Permanent
            }).ToList();

            // Đánh dấu leader preference cho 4 volunteer làm team leader
            volunteerProfiles[0].PreferredTeamRole = TeamRolePreference.Leader;
            volunteerProfiles[2].PreferredTeamRole = TeamRolePreference.Leader;
            volunteerProfiles[4].PreferredTeamRole = TeamRolePreference.Leader;
            volunteerProfiles[6].PreferredTeamRole = TeamRolePreference.Leader;

            context.VolunteerProfiles.AddRange(volunteerProfiles);
            await context.SaveChangesAsync();

            // ──────────────────────────────────────────────────
            // Seed 2 trạm
            // ──────────────────────────────────────────────────
            var stations = new List<ReliefStation>
            {
                new ReliefStation
                {
                    ReliefStationId    = Guid.NewGuid(),
                    Name               = "Trạm Cứu Trợ Trung Tâm Miền Trung - Đà Nẵng",
                    Level              = ReliefStationLevel.Regional,
                    LocationId         = await GetLocationId("da-nang", LocationLevel.Province),
                    Address            = "Đà Nẵng, Việt Nam",
                    ContactNumber      = "0236-3823-0001",
                    Longitude          = 108.2022,
                    CoverageRadiusKm   = 60,
                    Latitude           = 16.0544,
                    ReliefStationStatus= ReliefStationStatus.Active,
                    CreatedAt          = now,
                    UpdatedAt          = now
                },
                new ReliefStation
                {
                    ReliefStationId    = Guid.NewGuid(),
                    Name               = "Trạm Cứu Trợ Tỉnh Huế",
                    Level              = ReliefStationLevel.Provincial,
                    LocationId         = await GetLocationId("hue", LocationLevel.Province),
                    Address            = "Huế, Việt Nam",
                    ContactNumber      = "0234-3823-1002",
                    Longitude          = 107.5909,
                    CoverageRadiusKm   = 35,
                    Latitude           = 16.4637,
                    ReliefStationStatus= ReliefStationStatus.Active,
                    CreatedAt          = now,
                    UpdatedAt          = now
                }
            };


            context.ReliefStations.AddRange(stations);
            await context.SaveChangesAsync();

            var inventories = stations.Select(s => new Inventory
            {
                InventoryId = Guid.NewGuid(),
                ReliefStationId = s.ReliefStationId,
                Level = s.Level == ReliefStationLevel.Regional
                    ? InventoryLevel.Regional
                    : InventoryLevel.Provincial,
                Status = EntityStatus.Active
            }).ToList();

            context.Inventories.AddRange(inventories);

            // ──────────────────────────────────────────────────
            // Mỗi trạm có 2 team cứu hộ khẩn cấp trực thuộc
            // ──────────────────────────────────────────────────
            var daNangStationId = stations[0].ReliefStationId;
            var hueStationId = stations[1].ReliefStationId;

            var teamDn1 = new Team
            {
                TeamId = Guid.NewGuid(),
                Name = "Đội Cứu Hộ Khẩn Cấp ĐN-01",
                Description = "Đội phản ứng nhanh khu vực Đà Nẵng",
                ContactPhone = "0908000001",
                CreateBy = stationCoordinatorDn.Id,
                LeaderId = volunteers[0].Id,
                TeamType = TeamType.Rescue,
                Status = TeamStatus.Active,
                CreatedAt = now
            };

            var teamDn2 = new Team
            {
                TeamId = Guid.NewGuid(),
                Name = "Đội Cứu Hộ Khẩn Cấp ĐN-02",
                Description = "Đội hỗ trợ y tế sơ tán Đà Nẵng",
                ContactPhone = "0908000002",
                CreateBy = stationCoordinatorDn.Id,
                LeaderId = volunteers[2].Id,
                TeamType = TeamType.Rescue,
                Status = TeamStatus.Active,
                CreatedAt = now
            };

            var teamHue1 = new Team
            {
                TeamId = Guid.NewGuid(),
                Name = "Đội Cứu Hộ Khẩn Cấp Huế-01",
                Description = "Đội phản ứng nhanh khu vực Huế",
                ContactPhone = "0909000001",
                CreateBy = stationCoordinatorHue.Id,
                LeaderId = volunteers[4].Id,
                TeamType = TeamType.Rescue,
                Status = TeamStatus.Active,
                CreatedAt = now
            };

            var teamHue2 = new Team
            {
                TeamId = Guid.NewGuid(),
                Name = "Đội Cứu Hộ Khẩn Cấp Huế-02",
                Description = "Đội hỗ trợ cứu nạn đường thủy Huế",
                ContactPhone = "0909000002",
                CreateBy = stationCoordinatorHue.Id,
                LeaderId = volunteers[6].Id,
                TeamType = TeamType.Rescue,
                Status = TeamStatus.Active,
                CreatedAt = now
            };

            var teams = new List<Team> { teamDn1, teamDn2, teamHue1, teamHue2 };
            context.Teams.AddRange(teams);

            var stationTeams = new List<ReliefStationTeam>
            {
                new ReliefStationTeam
                {
                    ReliefStationTeamId = Guid.NewGuid(),
                    ReliefStationId = daNangStationId,
                    TeamId = teamDn1.TeamId,
                    Status = ReliefTeamAssignmentStatus.Approved,
                    Description = "Seed: team khẩn cấp trực thuộc trạm Đà Nẵng",
                    JoinedAt = now
                },
                new ReliefStationTeam
                {
                    ReliefStationTeamId = Guid.NewGuid(),
                    ReliefStationId = daNangStationId,
                    TeamId = teamDn2.TeamId,
                    Status = ReliefTeamAssignmentStatus.Approved,
                    Description = "Seed: team khẩn cấp trực thuộc trạm Đà Nẵng",
                    JoinedAt = now
                },
                new ReliefStationTeam
                {
                    ReliefStationTeamId = Guid.NewGuid(),
                    ReliefStationId = hueStationId,
                    TeamId = teamHue1.TeamId,
                    Status = ReliefTeamAssignmentStatus.Approved,
                    Description = "Seed: team khẩn cấp trực thuộc trạm Huế",
                    JoinedAt = now
                },
                new ReliefStationTeam
                {
                    ReliefStationTeamId = Guid.NewGuid(),
                    ReliefStationId = hueStationId,
                    TeamId = teamHue2.TeamId,
                    Status = ReliefTeamAssignmentStatus.Approved,
                    Description = "Seed: team khẩn cấp trực thuộc trạm Huế",
                    JoinedAt = now
                }
            };
            context.ReliefStationTeams.AddRange(stationTeams);

            // TeamMember: mỗi team có leader + 1 member
            var teamMembers = new List<TeamMember>
            {
                new TeamMember { TeamId = teamDn1.TeamId, UserId = volunteers[0].Id, RoleTeam = TeamRole.Leader, JoinedAt = now },
                new TeamMember { TeamId = teamDn1.TeamId, UserId = volunteers[1].Id, RoleTeam = TeamRole.Member, JoinedAt = now },

                new TeamMember { TeamId = teamDn2.TeamId, UserId = volunteers[2].Id, RoleTeam = TeamRole.Leader, JoinedAt = now },
                new TeamMember { TeamId = teamDn2.TeamId, UserId = volunteers[3].Id, RoleTeam = TeamRole.Member, JoinedAt = now },

                new TeamMember { TeamId = teamHue1.TeamId, UserId = volunteers[4].Id, RoleTeam = TeamRole.Leader, JoinedAt = now },
                new TeamMember { TeamId = teamHue1.TeamId, UserId = volunteers[5].Id, RoleTeam = TeamRole.Member, JoinedAt = now },

                new TeamMember { TeamId = teamHue2.TeamId, UserId = volunteers[6].Id, RoleTeam = TeamRole.Leader, JoinedAt = now },
                new TeamMember { TeamId = teamHue2.TeamId, UserId = volunteers[7].Id, RoleTeam = TeamRole.Member, JoinedAt = now }
            };
            context.TeamMembers.AddRange(teamMembers);

            await context.SaveChangesAsync();
        }
    }
}
