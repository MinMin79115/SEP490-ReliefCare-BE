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

            // ──────────────────────────────────────────────────
            // Seed 4 team cứu trợ (mỗi trạm 2 team), mỗi team 1 leader + 9 member
            // Kèm đầy đủ User, VolunteerProfile, VolunteerSkill, VolunteerCertificate
            // ──────────────────────────────────────────────────
            var skills = await context.Skills
                .OrderBy(s => s.Code)
                .ToListAsync();

            if (skills.Count == 0)
            {
                throw new Exception("Skills not found. Run SkillSeeder first.");
            }

            var rescueRoleId = await context.Roles
                .Where(r => r.Name == Role.Volunteer.ToString())
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (rescueRoleId == Guid.Empty)
            {
                throw new Exception("Role 'Volunteer' not found. Run RoleSeeder first.");
            }

            var reliefTeamDefinitions = new[]
            {
                new { Prefix = "dn.relief.01", TeamName = "Đội Cứu Trợ Đà Nẵng-01", Description = "Đội cứu trợ phân phối nhu yếu phẩm khu vực trung tâm Đà Nẵng", ContactPhone = "0908100001", StationId = daNangStationId, CreatorId = stationCoordinatorDn.Id },
                new { Prefix = "dn.relief.02", TeamName = "Đội Cứu Trợ Đà Nẵng-02", Description = "Đội hậu cần và hỗ trợ điểm phát tại Đà Nẵng", ContactPhone = "0908100002", StationId = daNangStationId, CreatorId = stationCoordinatorDn.Id },
                new { Prefix = "hue.relief.01", TeamName = "Đội Cứu Trợ Huế-01", Description = "Đội cứu trợ lưu động và phân phát hàng cho khu vực Huế", ContactPhone = "0909100001", StationId = hueStationId, CreatorId = stationCoordinatorHue.Id },
                new { Prefix = "hue.relief.02", TeamName = "Đội Cứu Trợ Huế-02", Description = "Đội hỗ trợ kho, vận chuyển và điểm phát khu vực Huế", ContactPhone = "0909100002", StationId = hueStationId, CreatorId = stationCoordinatorHue.Id }
            };

            var reliefVolunteerNames = new (string DisplayName, string Gender, string City)[]
            {
                ("Nguyễn Minh Phúc", "Male", "Đà Nẵng"),
                ("Trần Gia Hân", "Female", "Đà Nẵng"),
                ("Lê Quốc Đạt", "Male", "Đà Nẵng"),
                ("Phạm Thanh Vy", "Female", "Đà Nẵng"),
                ("Hoàng Tuấn Dũng", "Male", "Đà Nẵng"),
                ("Võ Khánh Linh", "Female", "Đà Nẵng"),
                ("Đặng Hải Nam", "Male", "Đà Nẵng"),
                ("Bùi Ngọc Trâm", "Female", "Đà Nẵng"),
                ("Phan Quang Huy", "Male", "Đà Nẵng"),
                ("Ngô Thuỳ Dương", "Female", "Đà Nẵng"),
                ("Trần Quốc Việt", "Male", "Huế"),
                ("Nguyễn Thảo Nhi", "Female", "Huế"),
                ("Lê Đức Mạnh", "Male", "Huế"),
                ("Phạm Quỳnh Anh", "Female", "Huế"),
                ("Hồ Hoàng Long", "Male", "Huế"),
                ("Đoàn Mỹ Tiên", "Female", "Huế"),
                ("Võ Đức Khang", "Male", "Huế"),
                ("Nguyễn Hà My", "Female", "Huế"),
                ("Phan Nhật Minh", "Male", "Huế"),
                ("Bùi Thanh Tâm", "Female", "Huế"),
                ("Lâm Anh Tú", "Male", "Đà Nẵng"),
                ("Nguyễn Diệu Linh", "Female", "Đà Nẵng"),
                ("Đặng Quốc Hưng", "Male", "Đà Nẵng"),
                ("Trương Bảo Ngọc", "Female", "Đà Nẵng"),
                ("Phạm Gia Bảo", "Male", "Đà Nẵng"),
                ("Võ Khả Hân", "Female", "Đà Nẵng"),
                ("Lê Tiến Thành", "Male", "Đà Nẵng"),
                ("Ngô Yến Nhi", "Female", "Đà Nẵng"),
                ("Trần Minh Khoa", "Male", "Đà Nẵng"),
                ("Đỗ Mai Anh", "Female", "Đà Nẵng"),
                ("Nguyễn Hữu Phước", "Male", "Huế"),
                ("Phan Thị Hồng", "Female", "Huế"),
                ("Lê Văn Sơn", "Male", "Huế"),
                ("Trần Mỹ Hạnh", "Female", "Huế"),
                ("Đặng Thanh Bình", "Male", "Huế"),
                ("Võ Bích Ngân", "Female", "Huế"),
                ("Hồ Minh Triết", "Male", "Huế"),
                ("Nguyễn Tú Uyên", "Female", "Huế"),
                ("Phạm Anh Vũ", "Male", "Huế"),
                ("Lê Khánh Hòa", "Female", "Huế")
            };

            var reliefUsers = new List<ApplicationUser>();
            var reliefProfiles = new List<VolunteerProfile>();
            var reliefUserRoles = new List<IdentityUserRole<Guid>>();
            var reliefCertificates = new List<VolunteerCertificate>();
            var reliefVolunteerSkills = new List<VolunteerSkill>();
            var reliefTeams = new List<Team>();
            var reliefStationTeams = new List<ReliefStationTeam>();
            var reliefTeamMembers = new List<TeamMember>();

            for (var teamIndex = 0; teamIndex < reliefTeamDefinitions.Length; teamIndex++)
            {
                var definition = reliefTeamDefinitions[teamIndex];
                var teamUsers = new List<ApplicationUser>();

                for (var memberIndex = 0; memberIndex < 10; memberIndex++)
                {
                    var person = reliefVolunteerNames[teamIndex * 10 + memberIndex];
                    var userName = $"{definition.Prefix}.m{memberIndex + 1:D2}";
                    var email = $"{userName}@relief.local";
                    var user = BuildUser(
                        userName,
                        email,
                        person.DisplayName,
                        $"09{teamIndex + 1}{memberIndex + 1:D7}",
                        person.City);

                    user.Gender = person.Gender;
                    user.DateOfBirth = new DateTime(1992 + ((teamIndex + memberIndex) % 8), (memberIndex % 12) + 1, ((memberIndex * 2) % 27) + 1, 0, 0, 0, DateTimeKind.Utc);

                    teamUsers.Add(user);
                    reliefUsers.Add(user);
                    reliefUserRoles.Add(new IdentityUserRole<Guid>
                    {
                        UserId = user.Id,
                        RoleId = rescueRoleId
                    });
                }

                var leader = teamUsers[0];
                var team = new Team
                {
                    TeamId = Guid.NewGuid(),
                    Name = definition.TeamName,
                    Description = definition.Description,
                    ContactPhone = definition.ContactPhone,
                    CreateBy = definition.CreatorId,
                    LeaderId = leader.Id,
                    TeamType = TeamType.Relief,
                    Status = TeamStatus.Active,
                    CreatedAt = now
                };

                reliefTeams.Add(team);
                reliefStationTeams.Add(new ReliefStationTeam
                {
                    ReliefStationTeamId = Guid.NewGuid(),
                    ReliefStationId = definition.StationId,
                    TeamId = team.TeamId,
                    Status = ReliefTeamAssignmentStatus.Approved,
                    Description = $"Seed: team cứu trợ trực thuộc {definition.TeamName}",
                    JoinedAt = now
                });

                for (var memberIndex = 0; memberIndex < teamUsers.Count; memberIndex++)
                {
                    var user = teamUsers[memberIndex];
                    var isLeader = memberIndex == 0;
                    var profile = new VolunteerProfile
                    {
                        VolunteerProfileId = Guid.NewGuid(),
                        UserId = user.Id,
                        VerificationStatus = VerificationStatus.Approved,
                        Status = VolunteerStatus.Active,
                        VerifiedAt = now,
                        CreatedAt = now,
                        Descriptions = isLeader
                            ? "Tình nguyện viên cứu trợ phụ trách điều phối đội, kho và phân phối tại điểm phát."
                            : "Tình nguyện viên cứu trợ tham gia đóng gói, vận chuyển và phân phối nhu yếu phẩm cho người dân.",
                        YearsOfExperience = isLeader ? 4 + (teamIndex % 3) : 1 + (memberIndex % 4),
                        PreferredTeamRole = isLeader
                            ? TeamRolePreference.Leader
                            : (memberIndex % 4 == 0 ? TeamRolePreference.Driver : TeamRolePreference.Member),
                        VolunteerType = memberIndex % 2 == 0 ? VolunteerType.Permanent : VolunteerType.Campaign,
                        Certificates = new List<VolunteerCertificate>(),
                        VolunteerSkills = new List<VolunteerSkill>()
                    };

                    reliefProfiles.Add(profile);

                    var selectedSkills = isLeader
                        ? skills.Take(3).ToList()
                        : memberIndex % 3 == 0
                            ? skills.Where(s => s.Code == "LOGISTICS" || s.Code == "FIRST_AID").ToList()
                            : memberIndex % 3 == 1
                                ? skills.Where(s => s.Code == "LOGISTICS" || s.Code == "MEDICAL_SUPPORT").ToList()
                                : skills.Where(s => s.Code == "LOGISTICS" || s.Code == "SEARCH_RESCUE").ToList();

                    foreach (var skill in selectedSkills.DistinctBy(s => s.SkillId))
                    {
                        reliefVolunteerSkills.Add(new VolunteerSkill
                        {
                            VolunteerProfileId = profile.VolunteerProfileId,
                            SkillId = skill.SkillId,
                            CreatedAt = now
                        });
                    }

                    reliefCertificates.Add(new VolunteerCertificate
                    {
                        CertificateId = Guid.NewGuid(),
                        VolunteerProfileId = profile.VolunteerProfileId,
                        Name = isLeader ? "Điều phối cứu trợ cộng đồng" : "An toàn phân phối cứu trợ",
                        IssuedBy = isLeader ? "Ban Điều phối Cứu trợ Miền Trung" : "Trung tâm Huấn luyện Tình nguyện viên",
                        IssuedDate = DateOnly.FromDateTime(now.AddMonths(-(6 + memberIndex))),
                        ExpiryDate = DateOnly.FromDateTime(now.AddYears(2)),
                        FileUrl = $"https://seed.relief.local/certificates/{user.UserName}-primary.pdf"
                    });

                    reliefCertificates.Add(new VolunteerCertificate
                    {
                        CertificateId = Guid.NewGuid(),
                        VolunteerProfileId = profile.VolunteerProfileId,
                        Name = memberIndex % 2 == 0 ? "Sơ cứu cơ bản" : "Quản lý kho cứu trợ",
                        IssuedBy = memberIndex % 2 == 0 ? "Hội Chữ thập đỏ" : "Sở Lao động và An sinh",
                        IssuedDate = DateOnly.FromDateTime(now.AddMonths(-(3 + memberIndex))),
                        ExpiryDate = memberIndex % 2 == 0 ? DateOnly.FromDateTime(now.AddYears(1)) : null,
                        FileUrl = $"https://seed.relief.local/certificates/{user.UserName}-secondary.pdf"
                    });

                    reliefTeamMembers.Add(new TeamMember
                    {
                        TeamId = team.TeamId,
                        UserId = user.Id,
                        RoleTeam = isLeader ? TeamRole.Leader : TeamRole.Member,
                        JoinedAt = now
                    });
                }
            }

            context.Users.AddRange(reliefUsers);
            await context.SaveChangesAsync();

            context.UserRoles.AddRange(reliefUserRoles);
            context.VolunteerProfiles.AddRange(reliefProfiles);
            context.Teams.AddRange(reliefTeams);
            context.ReliefStationTeams.AddRange(reliefStationTeams);
            context.VolunteerSkills.AddRange(reliefVolunteerSkills);
            context.VolunteerCertificates.AddRange(reliefCertificates);
            context.TeamMembers.AddRange(reliefTeamMembers);

            await context.SaveChangesAsync();
        }
    }
}
