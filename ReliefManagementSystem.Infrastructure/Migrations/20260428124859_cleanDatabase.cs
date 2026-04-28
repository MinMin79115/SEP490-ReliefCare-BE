using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class cleanDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    PrimaryKey = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                });

            migrationBuilder.CreateTable(
                name: "DisasterAnalysisLogs",
                columns: table => new
                {
                    DisasterAnalysisLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    RescueRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    LocationName = table.Column<string>(type: "text", nullable: false),
                    DisasterType = table.Column<int>(type: "integer", nullable: false),
                    RequestedModel = table.Column<string>(type: "text", nullable: true),
                    AdditionalContext = table.Column<string>(type: "text", nullable: true),
                    WeatherSnapshotJson = table.Column<string>(type: "text", nullable: false),
                    HeuristicRiskScore = table.Column<int>(type: "integer", nullable: false),
                    HeuristicRiskLevel = table.Column<string>(type: "text", nullable: false),
                    AssessmentConfidence = table.Column<string>(type: "text", nullable: false),
                    TriggerFactorsJson = table.Column<string>(type: "text", nullable: false),
                    PotentialScenariosJson = table.Column<string>(type: "text", nullable: false),
                    TopThreatsJson = table.Column<string>(type: "text", nullable: false),
                    DataLimitationNote = table.Column<string>(type: "text", nullable: true),
                    LlmProvider = table.Column<string>(type: "text", nullable: true),
                    LlmModel = table.Column<string>(type: "text", nullable: true),
                    PromptVersion = table.Column<string>(type: "text", nullable: true),
                    LlmResponseJson = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisasterAnalysisLogs", x => x.DisasterAnalysisLogId);
                });

            migrationBuilder.CreateTable(
                name: "Funds",
                columns: table => new
                {
                    FundId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TotalBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funds", x => x.FundId);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PopulationDensity = table.Column<decimal>(type: "numeric", nullable: false),
                    Area = table.Column<decimal>(type: "numeric", nullable: false),
                    Population = table.Column<long>(type: "bigint", nullable: false),
                    NormalizedName = table.Column<string>(type: "text", nullable: false),
                    Path = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.LocationId);
                    table.ForeignKey(
                        name: "FK_Locations_Locations_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PriorityCriterias",
                columns: table => new
                {
                    PriorityCriteriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Point = table.Column<int>(type: "integer", nullable: false),
                    DisasterType = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriorityCriterias", x => x.PriorityCriteriaId);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.SkillId);
                });

            migrationBuilder.CreateTable(
                name: "SupplyItems",
                columns: table => new
                {
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    EstimatedUnitCost = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyItems", x => x.SupplyItemId);
                });

            migrationBuilder.CreateTable(
                name: "VehicleTypes",
                columns: table => new
                {
                    VehicleTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DefaultCapacity = table.Column<int>(type: "integer", nullable: false),
                    CapacityKind = table.Column<int>(type: "integer", nullable: false),
                    CapacityUnit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleTypes", x => x.VehicleTypeId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReliefStations",
                columns: table => new
                {
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    ContactNumber = table.Column<string>(type: "text", nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    CoverageRadiusKm = table.Column<double>(type: "double precision", nullable: false),
                    ReliefStationStatus = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReliefStations", x => x.ReliefStationId);
                    table.ForeignKey(
                        name: "FK_ReliefStations_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PictureUrl = table.Column<string>(type: "text", nullable: true),
                    PicturePublicId = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    BanReason = table.Column<string>(type: "text", nullable: true),
                    ManagedStationReliefStationId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_ReliefStations_ManagedStationReliefStationId",
                        column: x => x.ManagedStationReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId");
                });

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.InventoryId);
                    table.ForeignKey(
                        name: "FK_Inventories_ReliefStations_ReliefStationId",
                        column: x => x.ReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: false),
                    AreaRadiusKm = table.Column<double>(type: "double precision", nullable: false),
                    AddressDetail = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    CompletionRule = table.Column<string>(type: "text", nullable: false),
                    AllowOverTarget = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    BudgetTotal = table.Column<decimal>(type: "numeric", nullable: false),
                    BudgetSpent = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.CampaignId);
                    table.ForeignKey(
                        name: "FK_Campaigns_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Campaigns_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailOtps",
                columns: table => new
                {
                    EmailOtpId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Purpose = table.Column<int>(type: "integer", nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailOtps", x => x.EmailOtpId);
                    table.ForeignKey(
                        name: "FK_EmailOtps_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManagerProfiles",
                columns: table => new
                {
                    ManagerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<string>(type: "text", nullable: false),
                    AssignedLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AppointedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagerProfiles", x => x.ManagerProfileId);
                    table.ForeignKey(
                        name: "FK_ManagerProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ManagerProfiles_Locations_AssignedLocationId",
                        column: x => x.AssignedLocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ModeratorProfiles",
                columns: table => new
                {
                    ModeratorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsStationHead = table.Column<bool>(type: "boolean", nullable: false),
                    AppointedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModeratorProfiles", x => x.ModeratorProfileId);
                    table.ForeignKey(
                        name: "FK_ModeratorProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModeratorProfiles_ReliefStations_ReliefStationId",
                        column: x => x.ReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MetadataJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    Expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Revoked = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByIp = table.Column<string>(type: "text", nullable: true),
                    Device = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Accuracy = table.Column<double>(type: "double precision", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: false),
                    ReporterUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReporterFullName = table.Column<string>(type: "text", nullable: false),
                    ReporterPhone = table.Column<string>(type: "text", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_Requests_AspNetUsers_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Requests_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ContactPhone = table.Column<string>(type: "text", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaderId = table.Column<Guid>(type: "uuid", nullable: true),
                    TeamType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.TeamId);
                    table.ForeignKey(
                        name: "FK_Teams_AspNetUsers_CreateBy",
                        column: x => x.CreateBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Teams_AspNetUsers_LeaderId",
                        column: x => x.LeaderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerProfiles",
                columns: table => new
                {
                    VolunteerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Descriptions = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    YearsOfExperience = table.Column<int>(type: "integer", nullable: true),
                    PreferredTeamRole = table.Column<int>(type: "integer", nullable: false),
                    VolunteerType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerProfiles", x => x.VolunteerProfileId);
                    table.ForeignKey(
                        name: "FK_VolunteerProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryStocks",
                columns: table => new
                {
                    InventoryStockId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentQuantity = table.Column<int>(type: "integer", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MinimumStockLevel = table.Column<int>(type: "integer", nullable: false),
                    MaximumStockLevel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryStocks", x => x.InventoryStockId);
                    table.CheckConstraint("CK_InventoryStocks_CurrentQuantity_NonNegative", "\"CurrentQuantity\" >= 0");
                    table.ForeignKey(
                        name: "FK_InventoryStocks_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "InventoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryStocks_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignBudgetTransfers",
                columns: table => new
                {
                    CampaignBudgetTransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceCampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetCampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    TransferredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransferredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignBudgetTransfers", x => x.CampaignBudgetTransferId);
                    table.CheckConstraint("CK_CampaignBudgetTransfers_Amount_Positive", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_CampaignBudgetTransfers_AspNetUsers_TransferredByUserId",
                        column: x => x.TransferredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignBudgetTransfers_Campaigns_SourceCampaignId",
                        column: x => x.SourceCampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignBudgetTransfers_Campaigns_TargetCampaignId",
                        column: x => x.TargetCampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignInventories",
                columns: table => new
                {
                    CampaignInventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignInventories", x => x.CampaignInventoryId);
                    table.ForeignKey(
                        name: "FK_CampaignInventories_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignResourceGoals",
                columns: table => new
                {
                    CampaignResourceGoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResourceType = table.Column<string>(type: "text", nullable: false),
                    TargetAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ReceivedAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsMet = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignResourceGoals", x => x.CampaignResourceGoalId);
                    table.ForeignKey(
                        name: "FK_CampaignResourceGoals_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignStations",
                columns: table => new
                {
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignStations", x => new { x.CampaignId, x.ReliefStationId });
                    table.ForeignKey(
                        name: "FK_CampaignStations_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignStations_ReliefStations_ReliefStationId",
                        column: x => x.ReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignVolunteerRegistrations",
                columns: table => new
                {
                    CampaignVolunteerRegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignVolunteerRegistrations", x => x.CampaignVolunteerRegistrationId);
                    table.ForeignKey(
                        name: "FK_CampaignVolunteerRegistrations_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignVolunteerRegistrations_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Donations",
                columns: table => new
                {
                    DonationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    DonorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DonorName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: true),
                    DonatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TransactionRef = table.Column<string>(type: "text", nullable: true),
                    PayOsOrderCode = table.Column<long>(type: "bigint", nullable: true),
                    PayOsPaymentLinkId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CheckoutUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GatewayResponse = table.Column<string>(type: "text", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donations", x => x.DonationId);
                    table.ForeignKey(
                        name: "FK_Donations_AspNetUsers_DonorUserId",
                        column: x => x.DonorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Donations_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReliefPackageDefinitions",
                columns: table => new
                {
                    ReliefPackageDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutputSupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CashSupportAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReliefPackageDefinitions", x => x.ReliefPackageDefinitionId);
                    table.ForeignKey(
                        name: "FK_ReliefPackageDefinitions_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReliefPackageDefinitions_SupplyItems_OutputSupplyItemId",
                        column: x => x.OutputSupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    AttachmentType = table.Column<int>(type: "integer", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.AttachmentId);
                    table.ForeignKey(
                        name: "FK_Attachments_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequestVerifications",
                columns: table => new
                {
                    RequestVerificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestVerifications", x => x.RequestVerificationId);
                    table.ForeignKey(
                        name: "FK_RequestVerifications_AspNetUsers_VerifiedBy",
                        column: x => x.VerifiedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequestVerifications_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RescueRequests",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    DisasterType = table.Column<string>(type: "text", nullable: false),
                    RescueRequestType = table.Column<int>(type: "integer", nullable: false),
                    PriorityPoint = table.Column<int>(type: "integer", nullable: true),
                    RescuePriorityLevel = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    WeatherCondition = table.Column<string>(type: "text", nullable: true),
                    WeatherTempC = table.Column<double>(type: "double precision", nullable: true),
                    WeatherWindKph = table.Column<double>(type: "double precision", nullable: true),
                    WeatherPrecipMm = table.Column<double>(type: "double precision", nullable: true),
                    WeatherVisibilityKm = table.Column<double>(type: "double precision", nullable: true),
                    WeatherRiskScore = table.Column<int>(type: "integer", nullable: true),
                    WeatherRiskLevel = table.Column<string>(type: "text", nullable: true),
                    WeatherObservedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StationToRequestDistanceKm = table.Column<double>(type: "double precision", nullable: true),
                    StationToRequestDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    StationToRequestDistanceMeters = table.Column<int>(type: "integer", nullable: true),
                    StationToRequestDurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    RescueRequestStatus = table.Column<string>(type: "text", nullable: false),
                    DispatchMode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RescueRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_RescueRequests_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RescueRequests_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignTeams",
                columns: table => new
                {
                    CampaignTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignTeams", x => x.CampaignTeamId);
                    table.ForeignKey(
                        name: "FK_CampaignTeams_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReliefStationTeams",
                columns: table => new
                {
                    ReliefStationTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RemovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReliefStationTeams", x => x.ReliefStationTeamId);
                    table.ForeignKey(
                        name: "FK_ReliefStationTeams_ReliefStations_ReliefStationId",
                        column: x => x.ReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReliefStationTeams_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RescueBatches",
                columns: table => new
                {
                    RescueBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RoutePolyline = table.Column<string>(type: "text", nullable: true),
                    TotalDistanceKm = table.Column<double>(type: "double precision", nullable: true),
                    EstimatedMinutes = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RescueBatches", x => x.RescueBatchId);
                    table.ForeignKey(
                        name: "FK_RescueBatches_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StationJoinRequests",
                columns: table => new
                {
                    StationJoinRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedByLeaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReviewNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedByModeratorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationJoinRequests", x => x.StationJoinRequestId);
                    table.ForeignKey(
                        name: "FK_StationJoinRequests_AspNetUsers_RequestedByLeaderId",
                        column: x => x.RequestedByLeaderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StationJoinRequests_AspNetUsers_ReviewedByModeratorId",
                        column: x => x.ReviewedByModeratorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StationJoinRequests_ReliefStations_ReliefStationId",
                        column: x => x.ReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StationJoinRequests_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamJoinRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    VolunteerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewNote = table.Column<string>(type: "text", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamJoinRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamJoinRequests_AspNetUsers_ReviewedBy",
                        column: x => x.ReviewedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TeamJoinRequests_AspNetUsers_VolunteerId",
                        column: x => x.VolunteerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamJoinRequests_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamMembers",
                columns: table => new
                {
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleTeam = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMembers", x => new { x.TeamId, x.UserId });
                    table.ForeignKey(
                        name: "FK_TeamMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamMembers_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: true),
                    LicensePlate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.VehicleId);
                    table.ForeignKey(
                        name: "FK_Vehicles_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vehicles_ReliefStations_ReliefStationId",
                        column: x => x.ReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vehicles_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vehicles_VehicleTypes_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "VehicleTypes",
                        principalColumn: "VehicleTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerCertificates",
                columns: table => new
                {
                    CertificateId = table.Column<Guid>(type: "uuid", nullable: false),
                    VolunteerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IssuedBy = table.Column<string>(type: "text", nullable: true),
                    IssuedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FileUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerCertificates", x => x.CertificateId);
                    table.ForeignKey(
                        name: "FK_VolunteerCertificates_VolunteerProfiles_VolunteerProfileId",
                        column: x => x.VolunteerProfileId,
                        principalTable: "VolunteerProfiles",
                        principalColumn: "VolunteerProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerSkills",
                columns: table => new
                {
                    VolunteerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerSkills", x => new { x.VolunteerProfileId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_VolunteerSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "SkillId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VolunteerSkills_VolunteerProfiles_VolunteerProfileId",
                        column: x => x.VolunteerProfileId,
                        principalTable: "VolunteerProfiles",
                        principalColumn: "VolunteerProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignInventoryStocks",
                columns: table => new
                {
                    CampaignInventoryStockId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignInventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentQuantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignInventoryStocks", x => x.CampaignInventoryStockId);
                    table.CheckConstraint("CK_CampaignInventoryStocks_CurrentQuantity_NonNegative", "\"CurrentQuantity\" >= 0");
                    table.ForeignKey(
                        name: "FK_CampaignInventoryStocks_CampaignInventories_CampaignInvento~",
                        column: x => x.CampaignInventoryId,
                        principalTable: "CampaignInventories",
                        principalColumn: "CampaignInventoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignInventoryStocks_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FundContributions",
                columns: table => new
                {
                    FundContributionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundId = table.Column<Guid>(type: "uuid", nullable: false),
                    DonationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    ContributedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundContributions", x => x.FundContributionId);
                    table.ForeignKey(
                        name: "FK_FundContributions_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FundContributions_Donations_DonationId",
                        column: x => x.DonationId,
                        principalTable: "Donations",
                        principalColumn: "DonationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FundContributions_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "FundId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    PaymentTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DonationId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OrderCode = table.Column<long>(type: "bigint", nullable: false),
                    PaymentLinkId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EventCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EventDescription = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TransactionDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CounterAccountName = table.Column<string>(type: "text", nullable: true),
                    CounterAccountNumber = table.Column<string>(type: "text", nullable: true),
                    CounterAccountBankName = table.Column<string>(type: "text", nullable: true),
                    VirtualAccountName = table.Column<string>(type: "text", nullable: true),
                    VirtualAccountNumber = table.Column<string>(type: "text", nullable: true),
                    RawPayload = table.Column<string>(type: "text", nullable: false),
                    Signature = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsSignatureValid = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.PaymentTransactionId);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Donations_DonationId",
                        column: x => x.DonationId,
                        principalTable: "Donations",
                        principalColumn: "DonationId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ReliefPackageAssemblies",
                columns: table => new
                {
                    ReliefPackageAssemblyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefPackageDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutputSupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityCreated = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReliefPackageAssemblies", x => x.ReliefPackageAssemblyId);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblies_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblies_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblies_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "InventoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblies_ReliefPackageDefinitions_ReliefPack~",
                        column: x => x.ReliefPackageDefinitionId,
                        principalTable: "ReliefPackageDefinitions",
                        principalColumn: "ReliefPackageDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblies_ReliefStations_ReliefStationId",
                        column: x => x.ReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblies_SupplyItems_OutputSupplyItemId",
                        column: x => x.OutputSupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReliefPackageDefinitionItems",
                columns: table => new
                {
                    ReliefPackageDefinitionItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefPackageDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReliefPackageDefinitionItems", x => x.ReliefPackageDefinitionItemId);
                    table.ForeignKey(
                        name: "FK_ReliefPackageDefinitionItems_ReliefPackageDefinitions_Relie~",
                        column: x => x.ReliefPackageDefinitionId,
                        principalTable: "ReliefPackageDefinitions",
                        principalColumn: "ReliefPackageDefinitionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReliefPackageDefinitionItems_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RescueRequestPriorities",
                columns: table => new
                {
                    RescueRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    PriorityCriteriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppliedPoint = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RescueRequestPriorities", x => new { x.RescueRequestId, x.PriorityCriteriaId });
                    table.ForeignKey(
                        name: "FK_RescueRequestPriorities_PriorityCriterias_PriorityCriteriaId",
                        column: x => x.PriorityCriteriaId,
                        principalTable: "PriorityCriterias",
                        principalColumn: "PriorityCriteriaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RescueRequestPriorities_RescueRequests_RescueRequestId",
                        column: x => x.RescueRequestId,
                        principalTable: "RescueRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignTasks",
                columns: table => new
                {
                    CampaignTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignTasks", x => x.CampaignTaskId);
                    table.ForeignKey(
                        name: "FK_CampaignTasks_CampaignTeams_CampaignTeamId",
                        column: x => x.CampaignTeamId,
                        principalTable: "CampaignTeams",
                        principalColumn: "CampaignTeamId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignTasks_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId");
                });

            migrationBuilder.CreateTable(
                name: "DistributionPoints",
                columns: table => new
                {
                    DistributionPointId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    DeliveryMode = table.Column<string>(type: "text", nullable: false),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributionPoints", x => x.DistributionPointId);
                    table.ForeignKey(
                        name: "FK_DistributionPoints_CampaignTeams_CampaignTeamId",
                        column: x => x.CampaignTeamId,
                        principalTable: "CampaignTeams",
                        principalColumn: "CampaignTeamId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DistributionPoints_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DistributionPoints_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DistributionPoints_ReliefStations_ReliefStationId",
                        column: x => x.ReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RescueBatchItems",
                columns: table => new
                {
                    RescueBatchItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    RescueBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RescueRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceOrder = table.Column<int>(type: "integer", nullable: false),
                    IsAutoAssigned = table.Column<bool>(type: "boolean", nullable: false),
                    DistanceKm = table.Column<double>(type: "double precision", nullable: true),
                    EstimatedMinutes = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RescueBatchItems", x => x.RescueBatchItemId);
                    table.ForeignKey(
                        name: "FK_RescueBatchItems_RescueBatches_RescueBatchId",
                        column: x => x.RescueBatchId,
                        principalTable: "RescueBatches",
                        principalColumn: "RescueBatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RescueBatchItems_RescueRequests_RescueRequestId",
                        column: x => x.RescueRequestId,
                        principalTable: "RescueRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignVehicles",
                columns: table => new
                {
                    CampaignVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedDriverId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignVehicles", x => x.CampaignVehicleId);
                    table.ForeignKey(
                        name: "FK_CampaignVehicles_CampaignTeams_CampaignTeamId",
                        column: x => x.CampaignTeamId,
                        principalTable: "CampaignTeams",
                        principalColumn: "CampaignTeamId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignVehicles_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignVehicles_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignVehicles_VolunteerProfiles_AssignedDriverId",
                        column: x => x.AssignedDriverId,
                        principalTable: "VolunteerProfiles",
                        principalColumn: "VolunteerProfileId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RescueOperations",
                columns: table => new
                {
                    RescueOperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RescueRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RescueOperations", x => x.RescueOperationId);
                    table.ForeignKey(
                        name: "FK_RescueOperations_ReliefStations_ReliefStationId",
                        column: x => x.ReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RescueOperations_RescueRequests_RescueRequestId",
                        column: x => x.RescueRequestId,
                        principalTable: "RescueRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RescueOperations_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RescueOperations_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SupplyTransfers",
                columns: table => new
                {
                    SupplyTransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransferCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ShippedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EvidenceUrls = table.Column<string>(type: "text", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: true),
                    DriverUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyTransfers", x => x.SupplyTransferId);
                    table.ForeignKey(
                        name: "FK_SupplyTransfers_AspNetUsers_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupplyTransfers_AspNetUsers_DriverUserId",
                        column: x => x.DriverUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupplyTransfers_AspNetUsers_RequestedBy",
                        column: x => x.RequestedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplyTransfers_ReliefStations_DestinationStationId",
                        column: x => x.DestinationStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplyTransfers_ReliefStations_SourceStationId",
                        column: x => x.SourceStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplyTransfers_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FundTransactions",
                columns: table => new
                {
                    FundTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric", nullable: false),
                    FundContributionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FundTransactions", x => x.FundTransactionId);
                    table.ForeignKey(
                        name: "FK_FundTransactions_FundContributions_FundContributionId",
                        column: x => x.FundContributionId,
                        principalTable: "FundContributions",
                        principalColumn: "FundContributionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FundTransactions_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "FundId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactionDetails",
                columns: table => new
                {
                    PaymentTransactionDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FieldValue = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactionDetails", x => x.PaymentTransactionDetailId);
                    table.ForeignKey(
                        name: "FK_PaymentTransactionDetails_PaymentTransactions_PaymentTransa~",
                        column: x => x.PaymentTransactionId,
                        principalTable: "PaymentTransactions",
                        principalColumn: "PaymentTransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReliefPackageAssemblyDetails",
                columns: table => new
                {
                    ReliefPackageAssemblyDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefPackageAssemblyId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityConsumed = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReliefPackageAssemblyDetails", x => x.ReliefPackageAssemblyDetailId);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblyDetails_ReliefPackageAssemblies_Relief~",
                        column: x => x.ReliefPackageAssemblyId,
                        principalTable: "ReliefPackageAssemblies",
                        principalColumn: "ReliefPackageAssemblyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblyDetails_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberTasks",
                columns: table => new
                {
                    MemberTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    VolunteerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubTaskTitle = table.Column<string>(type: "text", nullable: false),
                    TaskNote = table.Column<string>(type: "text", nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberTasks", x => x.MemberTaskId);
                    table.ForeignKey(
                        name: "FK_MemberTasks_CampaignTasks_CampaignTaskId",
                        column: x => x.CampaignTaskId,
                        principalTable: "CampaignTasks",
                        principalColumn: "CampaignTaskId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberTasks_VolunteerProfiles_VolunteerProfileId",
                        column: x => x.VolunteerProfileId,
                        principalTable: "VolunteerProfiles",
                        principalColumn: "VolunteerProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignHouseholds",
                columns: table => new
                {
                    CampaignHouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributionPointId = table.Column<Guid>(type: "uuid", nullable: true),
                    CampaignTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    HouseholdCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HeadOfHouseholdName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    HouseholdSize = table.Column<int>(type: "integer", nullable: false),
                    IsIsolated = table.Column<bool>(type: "boolean", nullable: false),
                    FloodSeverityLevel = table.Column<int>(type: "integer", nullable: true),
                    IsolationSeverityLevel = table.Column<int>(type: "integer", nullable: true),
                    RequiresBoat = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresLocalGuide = table.Column<bool>(type: "boolean", nullable: false),
                    DeliveryMode = table.Column<string>(type: "text", nullable: false),
                    FulfillmentStatus = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignHouseholds", x => x.CampaignHouseholdId);
                    table.ForeignKey(
                        name: "FK_CampaignHouseholds_CampaignTeams_CampaignTeamId",
                        column: x => x.CampaignTeamId,
                        principalTable: "CampaignTeams",
                        principalColumn: "CampaignTeamId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignHouseholds_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignHouseholds_DistributionPoints_DistributionPointId",
                        column: x => x.DistributionPointId,
                        principalTable: "DistributionPoints",
                        principalColumn: "DistributionPointId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignHouseholds_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SupplyShortageRequests",
                columns: table => new
                {
                    SupplyShortageRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributionPointId = table.Column<Guid>(type: "uuid", nullable: true),
                    CampaignTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyShortageRequests", x => x.SupplyShortageRequestId);
                    table.ForeignKey(
                        name: "FK_SupplyShortageRequests_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplyShortageRequests_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupplyShortageRequests_CampaignTeams_CampaignTeamId",
                        column: x => x.CampaignTeamId,
                        principalTable: "CampaignTeams",
                        principalColumn: "CampaignTeamId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupplyShortageRequests_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplyShortageRequests_DistributionPoints_DistributionPoint~",
                        column: x => x.DistributionPointId,
                        principalTable: "DistributionPoints",
                        principalColumn: "DistributionPointId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RescueOperationVehicles",
                columns: table => new
                {
                    RescueOperationVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    RescueOperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssignedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RescueOperationVehicles", x => x.RescueOperationVehicleId);
                    table.ForeignKey(
                        name: "FK_RescueOperationVehicles_RescueOperations_RescueOperationId",
                        column: x => x.RescueOperationId,
                        principalTable: "RescueOperations",
                        principalColumn: "RescueOperationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RescueOperationVehicles_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamTrackingPoints",
                columns: table => new
                {
                    TeamTrackingPointId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    RescueBatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    RescueOperationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    AccuracyMeters = table.Column<double>(type: "double precision", nullable: true),
                    SpeedKph = table.Column<double>(type: "double precision", nullable: true),
                    HeadingDegree = table.Column<double>(type: "double precision", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: false),
                    CapturedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamTrackingPoints", x => x.TeamTrackingPointId);
                    table.ForeignKey(
                        name: "FK_TeamTrackingPoints_RescueBatches_RescueBatchId",
                        column: x => x.RescueBatchId,
                        principalTable: "RescueBatches",
                        principalColumn: "RescueBatchId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TeamTrackingPoints_RescueOperations_RescueOperationId",
                        column: x => x.RescueOperationId,
                        principalTable: "RescueOperations",
                        principalColumn: "RescueOperationId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TeamTrackingPoints_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionCode = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    SupplyTransferId = table.Column<Guid>(type: "uuid", nullable: true),
                    ImportBatchCode = table.Column<string>(type: "text", nullable: true),
                    SourceReference = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "InventoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_SupplyTransfers_SupplyTransferId",
                        column: x => x.SupplyTransferId,
                        principalTable: "SupplyTransfers",
                        principalColumn: "SupplyTransferId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplyTransferDocuments",
                columns: table => new
                {
                    SupplyTransferDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyTransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyTransferDocuments", x => x.SupplyTransferDocumentId);
                    table.ForeignKey(
                        name: "FK_SupplyTransferDocuments_SupplyTransfers_SupplyTransferId",
                        column: x => x.SupplyTransferId,
                        principalTable: "SupplyTransfers",
                        principalColumn: "SupplyTransferId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplyTransferItems",
                columns: table => new
                {
                    SupplyTransferItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyTransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedQuantity = table.Column<int>(type: "integer", nullable: false),
                    ActualQuantity = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyTransferItems", x => x.SupplyTransferItemId);
                    table.ForeignKey(
                        name: "FK_SupplyTransferItems_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplyTransferItems_SupplyTransfers_SupplyTransferId",
                        column: x => x.SupplyTransferId,
                        principalTable: "SupplyTransfers",
                        principalColumn: "SupplyTransferId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplyTransferVehicles",
                columns: table => new
                {
                    SupplyTransferVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyTransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DepartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArrivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyTransferVehicles", x => x.SupplyTransferVehicleId);
                    table.ForeignKey(
                        name: "FK_SupplyTransferVehicles_AspNetUsers_DriverUserId",
                        column: x => x.DriverUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupplyTransferVehicles_SupplyTransfers_SupplyTransferId",
                        column: x => x.SupplyTransferId,
                        principalTable: "SupplyTransfers",
                        principalColumn: "SupplyTransferId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplyTransferVehicles_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "VehicleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HouseholdDeliveries",
                columns: table => new
                {
                    HouseholdDeliveryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignHouseholdId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributionPointId = table.Column<Guid>(type: "uuid", nullable: true),
                    CampaignTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReliefPackageDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeliveryMode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CashSupportAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseholdDeliveries", x => x.HouseholdDeliveryId);
                    table.ForeignKey(
                        name: "FK_HouseholdDeliveries_AspNetUsers_DeliveredByUserId",
                        column: x => x.DeliveredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HouseholdDeliveries_CampaignHouseholds_CampaignHouseholdId",
                        column: x => x.CampaignHouseholdId,
                        principalTable: "CampaignHouseholds",
                        principalColumn: "CampaignHouseholdId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HouseholdDeliveries_CampaignTeams_CampaignTeamId",
                        column: x => x.CampaignTeamId,
                        principalTable: "CampaignTeams",
                        principalColumn: "CampaignTeamId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HouseholdDeliveries_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HouseholdDeliveries_DistributionPoints_DistributionPointId",
                        column: x => x.DistributionPointId,
                        principalTable: "DistributionPoints",
                        principalColumn: "DistributionPointId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HouseholdDeliveries_ReliefPackageDefinitions_ReliefPackageD~",
                        column: x => x.ReliefPackageDefinitionId,
                        principalTable: "ReliefPackageDefinitions",
                        principalColumn: "ReliefPackageDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplyShortageRequestItems",
                columns: table => new
                {
                    SupplyShortageRequestItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyShortageRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityRequested = table.Column<int>(type: "integer", nullable: false),
                    QuantityApproved = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyShortageRequestItems", x => x.SupplyShortageRequestItemId);
                    table.ForeignKey(
                        name: "FK_SupplyShortageRequestItems_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplyShortageRequestItems_SupplyShortageRequests_SupplySho~",
                        column: x => x.SupplyShortageRequestId,
                        principalTable: "SupplyShortageRequests",
                        principalColumn: "SupplyShortageRequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InKindDonations",
                columns: table => new
                {
                    InKindDonationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DonorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: false),
                    DonorName = table.Column<string>(type: "text", nullable: true),
                    DonorContact = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    DonatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    InventoryTransactionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InKindDonations", x => x.InKindDonationId);
                    table.ForeignKey(
                        name: "FK_InKindDonations_AspNetUsers_DonorUserId",
                        column: x => x.DonorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InKindDonations_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InKindDonations_InventoryTransactions_InventoryTransactionId",
                        column: x => x.InventoryTransactionId,
                        principalTable: "InventoryTransactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InKindDonations_ReliefStations_ReliefStationId",
                        column: x => x.ReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactionItems",
                columns: table => new
                {
                    TransactionItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactionItems", x => x.TransactionItemId);
                    table.ForeignKey(
                        name: "FK_InventoryTransactionItems_InventoryTransactions_Transaction~",
                        column: x => x.TransactionId,
                        principalTable: "InventoryTransactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryTransactionItems_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementOrders",
                columns: table => new
                {
                    ProcurementOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationInventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderCode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TotalEstimatedCost = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalActualCost = table.Column<decimal>(type: "numeric", nullable: true),
                    SupplierName = table.Column<string>(type: "text", nullable: true),
                    SupplierContact = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ApprovalNote = table.Column<string>(type: "text", nullable: true),
                    ReceiveNote = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InventoryTransactionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementOrders", x => x.ProcurementOrderId);
                    table.ForeignKey(
                        name: "FK_ProcurementOrders_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcurementOrders_Inventories_DestinationInventoryId",
                        column: x => x.DestinationInventoryId,
                        principalTable: "Inventories",
                        principalColumn: "InventoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcurementOrders_InventoryTransactions_InventoryTransactio~",
                        column: x => x.InventoryTransactionId,
                        principalTable: "InventoryTransactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RescueOperationSupplies",
                columns: table => new
                {
                    RescueOperationSupplyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RescueOperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceInventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InventoryTransactionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RescueOperationSupplies", x => x.RescueOperationSupplyId);
                    table.ForeignKey(
                        name: "FK_RescueOperationSupplies_Inventories_SourceInventoryId",
                        column: x => x.SourceInventoryId,
                        principalTable: "Inventories",
                        principalColumn: "InventoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RescueOperationSupplies_InventoryTransactions_InventoryTran~",
                        column: x => x.InventoryTransactionId,
                        principalTable: "InventoryTransactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RescueOperationSupplies_RescueOperations_RescueOperationId",
                        column: x => x.RescueOperationId,
                        principalTable: "RescueOperations",
                        principalColumn: "RescueOperationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RescueOperationSupplies_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplyAllocations",
                columns: table => new
                {
                    AllocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceInventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InventoryTransactionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyAllocations", x => x.AllocationId);
                    table.ForeignKey(
                        name: "FK_SupplyAllocations_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplyAllocations_Inventories_SourceInventoryId",
                        column: x => x.SourceInventoryId,
                        principalTable: "Inventories",
                        principalColumn: "InventoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplyAllocations_InventoryTransactions_InventoryTransactio~",
                        column: x => x.InventoryTransactionId,
                        principalTable: "InventoryTransactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "HouseholdDeliveryProofs",
                columns: table => new
                {
                    HouseholdDeliveryProofId = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdDeliveryId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FileType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CapturedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseholdDeliveryProofs", x => x.HouseholdDeliveryProofId);
                    table.ForeignKey(
                        name: "FK_HouseholdDeliveryProofs_AspNetUsers_CapturedByUserId",
                        column: x => x.CapturedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HouseholdDeliveryProofs_HouseholdDeliveries_HouseholdDelive~",
                        column: x => x.HouseholdDeliveryId,
                        principalTable: "HouseholdDeliveries",
                        principalColumn: "HouseholdDeliveryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberTaskDeliveries",
                columns: table => new
                {
                    MemberTaskDeliveryId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    HouseholdDeliveryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedVolunteerProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberTaskDeliveries", x => x.MemberTaskDeliveryId);
                    table.ForeignKey(
                        name: "FK_MemberTaskDeliveries_HouseholdDeliveries_HouseholdDeliveryId",
                        column: x => x.HouseholdDeliveryId,
                        principalTable: "HouseholdDeliveries",
                        principalColumn: "HouseholdDeliveryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberTaskDeliveries_MemberTasks_MemberTaskId",
                        column: x => x.MemberTaskId,
                        principalTable: "MemberTasks",
                        principalColumn: "MemberTaskId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberTaskDeliveries_VolunteerProfiles_AssignedVolunteerPro~",
                        column: x => x.AssignedVolunteerProfileId,
                        principalTable: "VolunteerProfiles",
                        principalColumn: "VolunteerProfileId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InKindDonationDetails",
                columns: table => new
                {
                    InKindDonationDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    InKindDonationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InKindDonationDetails", x => x.InKindDonationDetailId);
                    table.ForeignKey(
                        name: "FK_InKindDonationDetails_InKindDonations_InKindDonationId",
                        column: x => x.InKindDonationId,
                        principalTable: "InKindDonations",
                        principalColumn: "InKindDonationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InKindDonationDetails_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementOrderItems",
                columns: table => new
                {
                    ProcurementOrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcurementOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "integer", nullable: true),
                    ActualUnitCost = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementOrderItems", x => x.ProcurementOrderItemId);
                    table.ForeignKey(
                        name: "FK_ProcurementOrderItems_ProcurementOrders_ProcurementOrderId",
                        column: x => x.ProcurementOrderId,
                        principalTable: "ProcurementOrders",
                        principalColumn: "ProcurementOrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcurementOrderItems_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignInventoryTransactions",
                columns: table => new
                {
                    CampaignInventoryTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignInventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    SupplyAllocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CampaignTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    DistributionPointId = table.Column<Guid>(type: "uuid", nullable: true),
                    HouseholdDeliveryId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReliefPackageDefinitionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignInventoryTransactions", x => x.CampaignInventoryTransactionId);
                    table.ForeignKey(
                        name: "FK_CampaignInventoryTransactions_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignInventoryTransactions_CampaignInventories_CampaignI~",
                        column: x => x.CampaignInventoryId,
                        principalTable: "CampaignInventories",
                        principalColumn: "CampaignInventoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignInventoryTransactions_SupplyAllocations_SupplyAlloc~",
                        column: x => x.SupplyAllocationId,
                        principalTable: "SupplyAllocations",
                        principalColumn: "AllocationId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SupplyAllocationItems",
                columns: table => new
                {
                    AllocationItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyAllocationItems", x => x.AllocationItemId);
                    table.ForeignKey(
                        name: "FK_SupplyAllocationItems_SupplyAllocations_AllocationId",
                        column: x => x.AllocationId,
                        principalTable: "SupplyAllocations",
                        principalColumn: "AllocationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplyAllocationItems_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignInventoryTransactionItems",
                columns: table => new
                {
                    CampaignInventoryTransactionItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignInventoryTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignInventoryTransactionItems", x => x.CampaignInventoryTransactionItemId);
                    table.ForeignKey(
                        name: "FK_CampaignInventoryTransactionItems_CampaignInventoryTransact~",
                        column: x => x.CampaignInventoryTransactionId,
                        principalTable: "CampaignInventoryTransactions",
                        principalColumn: "CampaignInventoryTransactionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignInventoryTransactionItems_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignTaskItems",
                columns: table => new
                {
                    CampaignTaskItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyAllocationItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityAssigned = table.Column<int>(type: "integer", nullable: false),
                    QuantityDelivered = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignTaskItems", x => x.CampaignTaskItemId);
                    table.ForeignKey(
                        name: "FK_CampaignTaskItems_CampaignTasks_CampaignTaskId",
                        column: x => x.CampaignTaskId,
                        principalTable: "CampaignTasks",
                        principalColumn: "CampaignTaskId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignTaskItems_SupplyAllocationItems_SupplyAllocationIte~",
                        column: x => x.SupplyAllocationItemId,
                        principalTable: "SupplyAllocationItems",
                        principalColumn: "AllocationItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberTaskItems",
                columns: table => new
                {
                    MemberTaskItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignTaskItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityAssigned = table.Column<int>(type: "integer", nullable: false),
                    QuantityDelivered = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberTaskItems", x => x.MemberTaskItemId);
                    table.ForeignKey(
                        name: "FK_MemberTaskItems_CampaignTaskItems_CampaignTaskItemId",
                        column: x => x.CampaignTaskItemId,
                        principalTable: "CampaignTaskItems",
                        principalColumn: "CampaignTaskItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberTaskItems_MemberTasks_MemberTaskId",
                        column: x => x.MemberTaskId,
                        principalTable: "MemberTasks",
                        principalColumn: "MemberTaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ManagedStationReliefStationId",
                table: "AspNetUsers",
                column: "ManagedStationReliefStationId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_RequestId",
                table: "Attachments",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignBudgetTransfers_SourceCampaignId_TargetCampaignId_T~",
                table: "CampaignBudgetTransfers",
                columns: new[] { "SourceCampaignId", "TargetCampaignId", "TransferredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignBudgetTransfers_TargetCampaignId",
                table: "CampaignBudgetTransfers",
                column: "TargetCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignBudgetTransfers_TransferredByUserId",
                table: "CampaignBudgetTransfers",
                column: "TransferredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignHouseholds_CampaignId_HouseholdCode",
                table: "CampaignHouseholds",
                columns: new[] { "CampaignId", "HouseholdCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignHouseholds_CampaignTeamId",
                table: "CampaignHouseholds",
                column: "CampaignTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignHouseholds_DistributionPointId",
                table: "CampaignHouseholds",
                column: "DistributionPointId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignHouseholds_LocationId",
                table: "CampaignHouseholds",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventories_CampaignId",
                table: "CampaignInventories",
                column: "CampaignId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryStocks_CampaignInventoryId_SupplyItemId",
                table: "CampaignInventoryStocks",
                columns: new[] { "CampaignInventoryId", "SupplyItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryStocks_SupplyItemId",
                table: "CampaignInventoryStocks",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactionItems_CampaignInventoryTransact~",
                table: "CampaignInventoryTransactionItems",
                column: "CampaignInventoryTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactionItems_SupplyItemId",
                table: "CampaignInventoryTransactionItems",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactions_CampaignInventoryId",
                table: "CampaignInventoryTransactions",
                column: "CampaignInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactions_CampaignTeamId",
                table: "CampaignInventoryTransactions",
                column: "CampaignTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactions_CreatedBy",
                table: "CampaignInventoryTransactions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactions_DistributionPointId",
                table: "CampaignInventoryTransactions",
                column: "DistributionPointId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactions_HouseholdDeliveryId",
                table: "CampaignInventoryTransactions",
                column: "HouseholdDeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactions_ReliefPackageDefinitionId",
                table: "CampaignInventoryTransactions",
                column: "ReliefPackageDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactions_SupplyAllocationId",
                table: "CampaignInventoryTransactions",
                column: "SupplyAllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignResourceGoals_CampaignId_ResourceType",
                table: "CampaignResourceGoals",
                columns: new[] { "CampaignId", "ResourceType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CreatedBy",
                table: "Campaigns",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_LocationId",
                table: "Campaigns",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignStations_ReliefStationId",
                table: "CampaignStations",
                column: "ReliefStationId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTaskItems_CampaignTaskId",
                table: "CampaignTaskItems",
                column: "CampaignTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTaskItems_SupplyAllocationItemId",
                table: "CampaignTaskItems",
                column: "SupplyAllocationItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTasks_CampaignId",
                table: "CampaignTasks",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTasks_CampaignTeamId",
                table: "CampaignTasks",
                column: "CampaignTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTeams_CampaignId_TeamId",
                table: "CampaignTeams",
                columns: new[] { "CampaignId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTeams_TeamId",
                table: "CampaignTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignVehicles_AssignedDriverId",
                table: "CampaignVehicles",
                column: "AssignedDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignVehicles_CampaignId",
                table: "CampaignVehicles",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignVehicles_CampaignTeamId",
                table: "CampaignVehicles",
                column: "CampaignTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignVehicles_VehicleId",
                table: "CampaignVehicles",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignVolunteerRegistrations_CampaignId_UserId_Status",
                table: "CampaignVolunteerRegistrations",
                columns: new[] { "CampaignId", "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignVolunteerRegistrations_UserId",
                table: "CampaignVolunteerRegistrations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionPoints_CampaignId_IsActive",
                table: "DistributionPoints",
                columns: new[] { "CampaignId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_DistributionPoints_CampaignTeamId",
                table: "DistributionPoints",
                column: "CampaignTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionPoints_LocationId",
                table: "DistributionPoints",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionPoints_ReliefStationId",
                table: "DistributionPoints",
                column: "ReliefStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_CampaignId",
                table: "Donations",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_DonorUserId",
                table: "Donations",
                column: "DonorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_PayOsOrderCode",
                table: "Donations",
                column: "PayOsOrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailOtps_UserId_Purpose_CreatedAt",
                table: "EmailOtps",
                columns: new[] { "UserId", "Purpose", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FundContributions_CampaignId",
                table: "FundContributions",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_FundContributions_DonationId",
                table: "FundContributions",
                column: "DonationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FundContributions_FundId",
                table: "FundContributions",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_IsDefault",
                table: "Funds",
                column: "IsDefault",
                filter: "\"IsDefault\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransactions_FundContributionId",
                table: "FundTransactions",
                column: "FundContributionId");

            migrationBuilder.CreateIndex(
                name: "IX_FundTransactions_FundId",
                table: "FundTransactions",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdDeliveries_CampaignHouseholdId",
                table: "HouseholdDeliveries",
                column: "CampaignHouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdDeliveries_CampaignId_CampaignTeamId_Status",
                table: "HouseholdDeliveries",
                columns: new[] { "CampaignId", "CampaignTeamId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdDeliveries_CampaignTeamId",
                table: "HouseholdDeliveries",
                column: "CampaignTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdDeliveries_DeliveredByUserId",
                table: "HouseholdDeliveries",
                column: "DeliveredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdDeliveries_DistributionPointId",
                table: "HouseholdDeliveries",
                column: "DistributionPointId");

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdDeliveries_ReliefPackageDefinitionId",
                table: "HouseholdDeliveries",
                column: "ReliefPackageDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdDeliveryProofs_CapturedByUserId",
                table: "HouseholdDeliveryProofs",
                column: "CapturedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HouseholdDeliveryProofs_HouseholdDeliveryId",
                table: "HouseholdDeliveryProofs",
                column: "HouseholdDeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_InKindDonationDetails_InKindDonationId",
                table: "InKindDonationDetails",
                column: "InKindDonationId");

            migrationBuilder.CreateIndex(
                name: "IX_InKindDonationDetails_SupplyItemId",
                table: "InKindDonationDetails",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InKindDonations_CampaignId",
                table: "InKindDonations",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_InKindDonations_DonorUserId",
                table: "InKindDonations",
                column: "DonorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InKindDonations_InventoryTransactionId",
                table: "InKindDonations",
                column: "InventoryTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_InKindDonations_ReliefStationId",
                table: "InKindDonations",
                column: "ReliefStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_ReliefStationId",
                table: "Inventories",
                column: "ReliefStationId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_InventoryId_SupplyItemId",
                table: "InventoryStocks",
                columns: new[] { "InventoryId", "SupplyItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_SupplyItemId",
                table: "InventoryStocks",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionItems_SupplyItemId",
                table: "InventoryTransactionItems",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionItems_TransactionId",
                table: "InventoryTransactionItems",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_CreatedBy",
                table: "InventoryTransactions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_InventoryId",
                table: "InventoryTransactions",
                column: "InventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_SupplyTransferId",
                table: "InventoryTransactions",
                column: "SupplyTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_TransactionCode",
                table: "InventoryTransactions",
                column: "TransactionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_ParentId",
                table: "Locations",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerProfiles_AssignedLocationId",
                table: "ManagerProfiles",
                column: "AssignedLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerProfiles_UserId",
                table: "ManagerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberTaskDeliveries_AssignedVolunteerProfileId",
                table: "MemberTaskDeliveries",
                column: "AssignedVolunteerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberTaskDeliveries_HouseholdDeliveryId",
                table: "MemberTaskDeliveries",
                column: "HouseholdDeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberTaskDeliveries_MemberTaskId_HouseholdDeliveryId",
                table: "MemberTaskDeliveries",
                columns: new[] { "MemberTaskId", "HouseholdDeliveryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberTaskItems_CampaignTaskItemId",
                table: "MemberTaskItems",
                column: "CampaignTaskItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberTaskItems_MemberTaskId",
                table: "MemberTaskItems",
                column: "MemberTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberTasks_CampaignTaskId",
                table: "MemberTasks",
                column: "CampaignTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberTasks_VolunteerProfileId",
                table: "MemberTasks",
                column: "VolunteerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ModeratorProfiles_ReliefStationId",
                table: "ModeratorProfiles",
                column: "ReliefStationId");

            migrationBuilder.CreateIndex(
                name: "IX_ModeratorProfiles_UserId",
                table: "ModeratorProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId_IsRead",
                table: "Notifications",
                columns: new[] { "RecipientId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactionDetails_PaymentTransactionId",
                table: "PaymentTransactionDetails",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_DonationId",
                table: "PaymentTransactions",
                column: "DonationId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_Provider_OrderCode_PaymentLinkId",
                table: "PaymentTransactions",
                columns: new[] { "Provider", "OrderCode", "PaymentLinkId" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_Provider_Reference",
                table: "PaymentTransactions",
                columns: new[] { "Provider", "Reference" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_UserId",
                table: "PaymentTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PriorityCriterias_Code",
                table: "PriorityCriterias",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementOrderItems_ProcurementOrderId",
                table: "ProcurementOrderItems",
                column: "ProcurementOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementOrderItems_SupplyItemId",
                table: "ProcurementOrderItems",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementOrders_CampaignId",
                table: "ProcurementOrders",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementOrders_DestinationInventoryId",
                table: "ProcurementOrders",
                column: "DestinationInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementOrders_InventoryTransactionId",
                table: "ProcurementOrders",
                column: "InventoryTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblies_CampaignId",
                table: "ReliefPackageAssemblies",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblies_CreatedBy",
                table: "ReliefPackageAssemblies",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblies_InventoryId_CreatedAt",
                table: "ReliefPackageAssemblies",
                columns: new[] { "InventoryId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblies_OutputSupplyItemId",
                table: "ReliefPackageAssemblies",
                column: "OutputSupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblies_ReliefPackageDefinitionId_CreatedAt",
                table: "ReliefPackageAssemblies",
                columns: new[] { "ReliefPackageDefinitionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblies_ReliefStationId",
                table: "ReliefPackageAssemblies",
                column: "ReliefStationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblyDetails_ReliefPackageAssemblyId_Supply~",
                table: "ReliefPackageAssemblyDetails",
                columns: new[] { "ReliefPackageAssemblyId", "SupplyItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblyDetails_SupplyItemId",
                table: "ReliefPackageAssemblyDetails",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageDefinitionItems_ReliefPackageDefinitionId_Supp~",
                table: "ReliefPackageDefinitionItems",
                columns: new[] { "ReliefPackageDefinitionId", "SupplyItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageDefinitionItems_SupplyItemId",
                table: "ReliefPackageDefinitionItems",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageDefinitions_CampaignId_Name",
                table: "ReliefPackageDefinitions",
                columns: new[] { "CampaignId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageDefinitions_OutputSupplyItemId",
                table: "ReliefPackageDefinitions",
                column: "OutputSupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefStations_LocationId",
                table: "ReliefStations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefStationTeams_ReliefStationId_TeamId",
                table: "ReliefStationTeams",
                columns: new[] { "ReliefStationId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReliefStationTeams_TeamId",
                table: "ReliefStationTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_LocationId",
                table: "Requests",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ReporterUserId",
                table: "Requests",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestVerifications_RequestId",
                table: "RequestVerifications",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestVerifications_VerifiedBy",
                table: "RequestVerifications",
                column: "VerifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RescueBatches_TeamId_IsActive",
                table: "RescueBatches",
                columns: new[] { "TeamId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_RescueBatchItems_RescueBatchId_SequenceOrder",
                table: "RescueBatchItems",
                columns: new[] { "RescueBatchId", "SequenceOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_RescueBatchItems_RescueRequestId",
                table: "RescueBatchItems",
                column: "RescueRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperations_ReliefStationId",
                table: "RescueOperations",
                column: "ReliefStationId");

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperations_RescueRequestId",
                table: "RescueOperations",
                column: "RescueRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperations_TeamId",
                table: "RescueOperations",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperations_VehicleId",
                table: "RescueOperations",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperationSupplies_InventoryTransactionId",
                table: "RescueOperationSupplies",
                column: "InventoryTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperationSupplies_RescueOperationId",
                table: "RescueOperationSupplies",
                column: "RescueOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperationSupplies_SourceInventoryId",
                table: "RescueOperationSupplies",
                column: "SourceInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperationSupplies_SupplyItemId",
                table: "RescueOperationSupplies",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperationVehicles_RescueOperationId_VehicleId",
                table: "RescueOperationVehicles",
                columns: new[] { "RescueOperationId", "VehicleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperationVehicles_VehicleId",
                table: "RescueOperationVehicles",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_RescueRequestPriorities_PriorityCriteriaId",
                table: "RescueRequestPriorities",
                column: "PriorityCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_RescueRequests_CampaignId",
                table: "RescueRequests",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_StationJoinRequests_ReliefStationId",
                table: "StationJoinRequests",
                column: "ReliefStationId");

            migrationBuilder.CreateIndex(
                name: "IX_StationJoinRequests_RequestedByLeaderId",
                table: "StationJoinRequests",
                column: "RequestedByLeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_StationJoinRequests_ReviewedByModeratorId",
                table: "StationJoinRequests",
                column: "ReviewedByModeratorId");

            migrationBuilder.CreateIndex(
                name: "IX_StationJoinRequests_TeamId_ReliefStationId_Status",
                table: "StationJoinRequests",
                columns: new[] { "TeamId", "ReliefStationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplyAllocationItems_AllocationId",
                table: "SupplyAllocationItems",
                column: "AllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyAllocationItems_SupplyItemId",
                table: "SupplyAllocationItems",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyAllocations_CampaignId",
                table: "SupplyAllocations",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyAllocations_InventoryTransactionId",
                table: "SupplyAllocations",
                column: "InventoryTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplyAllocations_SourceInventoryId",
                table: "SupplyAllocations",
                column: "SourceInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyShortageRequestItems_SupplyItemId",
                table: "SupplyShortageRequestItems",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyShortageRequestItems_SupplyShortageRequestId",
                table: "SupplyShortageRequestItems",
                column: "SupplyShortageRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyShortageRequests_CampaignId_Status_RequestedAt",
                table: "SupplyShortageRequests",
                columns: new[] { "CampaignId", "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplyShortageRequests_CampaignTeamId",
                table: "SupplyShortageRequests",
                column: "CampaignTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyShortageRequests_DistributionPointId",
                table: "SupplyShortageRequests",
                column: "DistributionPointId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyShortageRequests_RequestedByUserId",
                table: "SupplyShortageRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyShortageRequests_ReviewedByUserId",
                table: "SupplyShortageRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransferDocuments_SupplyTransferId_DocumentType",
                table: "SupplyTransferDocuments",
                columns: new[] { "SupplyTransferId", "DocumentType" },
                unique: true,
                filter: "\"IsCurrent\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransferDocuments_SupplyTransferId_DocumentType_Versi~",
                table: "SupplyTransferDocuments",
                columns: new[] { "SupplyTransferId", "DocumentType", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransferItems_SupplyItemId",
                table: "SupplyTransferItems",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransferItems_SupplyTransferId",
                table: "SupplyTransferItems",
                column: "SupplyTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransfers_ApprovedBy",
                table: "SupplyTransfers",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransfers_DestinationStationId",
                table: "SupplyTransfers",
                column: "DestinationStationId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransfers_DriverUserId",
                table: "SupplyTransfers",
                column: "DriverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransfers_RequestedBy",
                table: "SupplyTransfers",
                column: "RequestedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransfers_SourceStationId",
                table: "SupplyTransfers",
                column: "SourceStationId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransfers_TransferCode",
                table: "SupplyTransfers",
                column: "TransferCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransfers_VehicleId",
                table: "SupplyTransfers",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransferVehicles_DriverUserId",
                table: "SupplyTransferVehicles",
                column: "DriverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransferVehicles_SupplyTransferId_VehicleId",
                table: "SupplyTransferVehicles",
                columns: new[] { "SupplyTransferId", "VehicleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransferVehicles_VehicleId_Status",
                table: "SupplyTransferVehicles",
                columns: new[] { "VehicleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_TeamJoinRequests_ReviewedBy",
                table: "TeamJoinRequests",
                column: "ReviewedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TeamJoinRequests_TeamId",
                table: "TeamJoinRequests",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamJoinRequests_VolunteerId",
                table: "TeamJoinRequests",
                column: "VolunteerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_UserId",
                table: "TeamMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_CreateBy",
                table: "Teams",
                column: "CreateBy");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_LeaderId",
                table: "Teams",
                column: "LeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamTrackingPoints_RescueBatchId_CapturedAtUtc",
                table: "TeamTrackingPoints",
                columns: new[] { "RescueBatchId", "CapturedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TeamTrackingPoints_RescueOperationId_CapturedAtUtc",
                table: "TeamTrackingPoints",
                columns: new[] { "RescueOperationId", "CapturedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TeamTrackingPoints_TeamId_CapturedAtUtc",
                table: "TeamTrackingPoints",
                columns: new[] { "TeamId", "CapturedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CreatedBy",
                table: "Vehicles",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_LicensePlate",
                table: "Vehicles",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_ReliefStationId",
                table: "Vehicles",
                column: "ReliefStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_TeamId",
                table: "Vehicles",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_VehicleTypeId",
                table: "Vehicles",
                column: "VehicleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleTypes_TypeName",
                table: "VehicleTypes",
                column: "TypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerCertificates_VolunteerProfileId",
                table: "VolunteerCertificates",
                column: "VolunteerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerProfiles_UserId",
                table: "VolunteerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerSkills_SkillId",
                table: "VolunteerSkills",
                column: "SkillId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CampaignBudgetTransfers");

            migrationBuilder.DropTable(
                name: "CampaignInventoryStocks");

            migrationBuilder.DropTable(
                name: "CampaignInventoryTransactionItems");

            migrationBuilder.DropTable(
                name: "CampaignResourceGoals");

            migrationBuilder.DropTable(
                name: "CampaignStations");

            migrationBuilder.DropTable(
                name: "CampaignVehicles");

            migrationBuilder.DropTable(
                name: "CampaignVolunteerRegistrations");

            migrationBuilder.DropTable(
                name: "DisasterAnalysisLogs");

            migrationBuilder.DropTable(
                name: "EmailOtps");

            migrationBuilder.DropTable(
                name: "FundTransactions");

            migrationBuilder.DropTable(
                name: "HouseholdDeliveryProofs");

            migrationBuilder.DropTable(
                name: "InKindDonationDetails");

            migrationBuilder.DropTable(
                name: "InventoryStocks");

            migrationBuilder.DropTable(
                name: "InventoryTransactionItems");

            migrationBuilder.DropTable(
                name: "ManagerProfiles");

            migrationBuilder.DropTable(
                name: "MemberTaskDeliveries");

            migrationBuilder.DropTable(
                name: "MemberTaskItems");

            migrationBuilder.DropTable(
                name: "ModeratorProfiles");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PaymentTransactionDetails");

            migrationBuilder.DropTable(
                name: "ProcurementOrderItems");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "ReliefPackageAssemblyDetails");

            migrationBuilder.DropTable(
                name: "ReliefPackageDefinitionItems");

            migrationBuilder.DropTable(
                name: "ReliefStationTeams");

            migrationBuilder.DropTable(
                name: "RequestVerifications");

            migrationBuilder.DropTable(
                name: "RescueBatchItems");

            migrationBuilder.DropTable(
                name: "RescueOperationSupplies");

            migrationBuilder.DropTable(
                name: "RescueOperationVehicles");

            migrationBuilder.DropTable(
                name: "RescueRequestPriorities");

            migrationBuilder.DropTable(
                name: "StationJoinRequests");

            migrationBuilder.DropTable(
                name: "SupplyShortageRequestItems");

            migrationBuilder.DropTable(
                name: "SupplyTransferDocuments");

            migrationBuilder.DropTable(
                name: "SupplyTransferItems");

            migrationBuilder.DropTable(
                name: "SupplyTransferVehicles");

            migrationBuilder.DropTable(
                name: "TeamJoinRequests");

            migrationBuilder.DropTable(
                name: "TeamMembers");

            migrationBuilder.DropTable(
                name: "TeamTrackingPoints");

            migrationBuilder.DropTable(
                name: "VolunteerCertificates");

            migrationBuilder.DropTable(
                name: "VolunteerSkills");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "CampaignInventoryTransactions");

            migrationBuilder.DropTable(
                name: "FundContributions");

            migrationBuilder.DropTable(
                name: "InKindDonations");

            migrationBuilder.DropTable(
                name: "HouseholdDeliveries");

            migrationBuilder.DropTable(
                name: "CampaignTaskItems");

            migrationBuilder.DropTable(
                name: "MemberTasks");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "ProcurementOrders");

            migrationBuilder.DropTable(
                name: "ReliefPackageAssemblies");

            migrationBuilder.DropTable(
                name: "PriorityCriterias");

            migrationBuilder.DropTable(
                name: "SupplyShortageRequests");

            migrationBuilder.DropTable(
                name: "RescueBatches");

            migrationBuilder.DropTable(
                name: "RescueOperations");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "CampaignInventories");

            migrationBuilder.DropTable(
                name: "Funds");

            migrationBuilder.DropTable(
                name: "CampaignHouseholds");

            migrationBuilder.DropTable(
                name: "SupplyAllocationItems");

            migrationBuilder.DropTable(
                name: "CampaignTasks");

            migrationBuilder.DropTable(
                name: "VolunteerProfiles");

            migrationBuilder.DropTable(
                name: "Donations");

            migrationBuilder.DropTable(
                name: "ReliefPackageDefinitions");

            migrationBuilder.DropTable(
                name: "RescueRequests");

            migrationBuilder.DropTable(
                name: "DistributionPoints");

            migrationBuilder.DropTable(
                name: "SupplyAllocations");

            migrationBuilder.DropTable(
                name: "SupplyItems");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "CampaignTeams");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropTable(
                name: "SupplyTransfers");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "VehicleTypes");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ReliefStations");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
