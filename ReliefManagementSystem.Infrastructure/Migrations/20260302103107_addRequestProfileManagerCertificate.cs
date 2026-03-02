using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addRequestProfileManagerCertificate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_ReliefStations_ReliefStationId1",
                table: "Inventories");

            migrationBuilder.DropForeignKey(
                name: "FK_ReliefStations_AspNetUsers_ManagerId",
                table: "ReliefStations");

            migrationBuilder.DropForeignKey(
                name: "FK_ReliefStations_Locations_LocationId",
                table: "ReliefStations");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_ReliefStationId1",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "ReliefStationId1",
                table: "Inventories");

            migrationBuilder.AddColumn<int>(
                name: "YearsOfExperience",
                table: "VolunteerProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AreaRadiusKm",
                table: "Campaigns",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "BudgetSpent",
                table: "Campaigns",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BudgetTotal",
                table: "Campaigns",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Campaigns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Campaigns",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Campaigns",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Campaigns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ManagedStationReliefStationId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: true);

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
                });

            migrationBuilder.CreateTable(
                name: "VolunteerCertificates",
                columns: table => new
                {
                    CertificateId = table.Column<Guid>(type: "uuid", nullable: false),
                    VolunteerProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IssuedBy = table.Column<string>(type: "text", nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                name: "Attachments",
                columns: table => new
                {
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true),
                    FileType = table.Column<string>(type: "text", nullable: true),
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
                name: "ReliefRequests",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReliefRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_ReliefRequests_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReliefRequests_Requests_RequestId",
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
                    Method = table.Column<string>(type: "text", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: false),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false)
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
                    DisasterType = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    RescueRequestStatus = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RescueRequests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_RescueRequests_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReliefNeedItems",
                columns: table => new
                {
                    ReliefNeedItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    NeedType = table.Column<string>(type: "text", nullable: false),
                    UrgencyLevel = table.Column<string>(type: "text", nullable: false),
                    PeopleCount = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReliefNeedItems", x => x.ReliefNeedItemId);
                    table.ForeignKey(
                        name: "FK_ReliefNeedItems_ReliefRequests_ReliefRequestId",
                        column: x => x.ReliefRequestId,
                        principalTable: "ReliefRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RescueOperations",
                columns: table => new
                {
                    RescueOperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RescueRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CreatedBy",
                table: "Campaigns",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ManagedStationReliefStationId",
                table: "AspNetUsers",
                column: "ManagedStationReliefStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_RequestId",
                table: "Attachments",
                column: "RequestId");

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
                name: "IX_PriorityCriterias_Code",
                table: "PriorityCriterias",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReliefNeedItems_ReliefRequestId",
                table: "ReliefNeedItems",
                column: "ReliefRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefRequests_CampaignId",
                table: "ReliefRequests",
                column: "CampaignId");

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
                name: "IX_RescueRequestPriorities_PriorityCriteriaId",
                table: "RescueRequestPriorities",
                column: "PriorityCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerCertificates_VolunteerProfileId",
                table: "VolunteerCertificates",
                column: "VolunteerProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ReliefStations_ManagedStationReliefStationId",
                table: "AspNetUsers",
                column: "ManagedStationReliefStationId",
                principalTable: "ReliefStations",
                principalColumn: "ReliefStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_AspNetUsers_CreatedBy",
                table: "Campaigns",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReliefStations_AspNetUsers_ManagerId",
                table: "ReliefStations",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ReliefStations_Locations_LocationId",
                table: "ReliefStations",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ReliefStations_ManagedStationReliefStationId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_AspNetUsers_CreatedBy",
                table: "Campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_ReliefStations_AspNetUsers_ManagerId",
                table: "ReliefStations");

            migrationBuilder.DropForeignKey(
                name: "FK_ReliefStations_Locations_LocationId",
                table: "ReliefStations");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "ManagerProfiles");

            migrationBuilder.DropTable(
                name: "ReliefNeedItems");

            migrationBuilder.DropTable(
                name: "RequestVerifications");

            migrationBuilder.DropTable(
                name: "RescueOperations");

            migrationBuilder.DropTable(
                name: "RescueRequestPriorities");

            migrationBuilder.DropTable(
                name: "VolunteerCertificates");

            migrationBuilder.DropTable(
                name: "ReliefRequests");

            migrationBuilder.DropTable(
                name: "PriorityCriterias");

            migrationBuilder.DropTable(
                name: "RescueRequests");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_CreatedBy",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ManagedStationReliefStationId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "YearsOfExperience",
                table: "VolunteerProfiles");

            migrationBuilder.DropColumn(
                name: "AreaRadiusKm",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "BudgetSpent",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "BudgetTotal",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ManagedStationReliefStationId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "ReliefStationId1",
                table: "Inventories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_ReliefStationId1",
                table: "Inventories",
                column: "ReliefStationId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_ReliefStations_ReliefStationId1",
                table: "Inventories",
                column: "ReliefStationId1",
                principalTable: "ReliefStations",
                principalColumn: "ReliefStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReliefStations_AspNetUsers_ManagerId",
                table: "ReliefStations",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReliefStations_Locations_LocationId",
                table: "ReliefStations",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
