using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReliefDistributionWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistributionSessionItems");

            migrationBuilder.DropTable(
                name: "DistributionSessionRequests");

            migrationBuilder.DropTable(
                name: "ReliefFulfillmentItems");

            migrationBuilder.DropTable(
                name: "ReliefNeedItems");

            migrationBuilder.DropTable(
                name: "ReliefFulfillments");

            migrationBuilder.DropTable(
                name: "DistributionSessions");

            migrationBuilder.DropTable(
                name: "ReliefRequests");

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
                name: "ReliefPackageDefinitions",
                columns: table => new
                {
                    ReliefPackageDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HouseholdDeliveryProofs");

            migrationBuilder.DropTable(
                name: "ReliefPackageDefinitionItems");

            migrationBuilder.DropTable(
                name: "SupplyShortageRequestItems");

            migrationBuilder.DropTable(
                name: "HouseholdDeliveries");

            migrationBuilder.DropTable(
                name: "SupplyShortageRequests");

            migrationBuilder.DropTable(
                name: "CampaignHouseholds");

            migrationBuilder.DropTable(
                name: "ReliefPackageDefinitions");

            migrationBuilder.DropTable(
                name: "DistributionPoints");

            migrationBuilder.CreateTable(
                name: "DistributionSessions",
                columns: table => new
                {
                    DistributionSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    LocationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Mode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RadiusMeters = table.Column<double>(type: "double precision", nullable: true),
                    ScheduledEndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduledStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributionSessions", x => x.DistributionSessionId);
                    table.ForeignKey(
                        name: "FK_DistributionSessions_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DistributionSessions_ReliefStations_ReliefStationId",
                        column: x => x.ReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReliefRequests",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedReliefStationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                        name: "FK_ReliefRequests_ReliefStations_AssignedReliefStationId",
                        column: x => x.AssignedReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReliefRequests_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DistributionSessionItems",
                columns: table => new
                {
                    DistributionSessionItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributionSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyAllocationItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeliveredQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    ReservedQuantity = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributionSessionItems", x => x.DistributionSessionItemId);
                    table.ForeignKey(
                        name: "FK_DistributionSessionItems_DistributionSessions_DistributionS~",
                        column: x => x.DistributionSessionId,
                        principalTable: "DistributionSessions",
                        principalColumn: "DistributionSessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DistributionSessionItems_SupplyAllocationItems_SupplyAlloca~",
                        column: x => x.SupplyAllocationItemId,
                        principalTable: "SupplyAllocationItems",
                        principalColumn: "AllocationItemId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DistributionSessionItems_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DistributionSessionRequests",
                columns: table => new
                {
                    DistributionSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlannedNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributionSessionRequests", x => new { x.DistributionSessionId, x.ReliefRequestId });
                    table.ForeignKey(
                        name: "FK_DistributionSessionRequests_DistributionSessions_Distributi~",
                        column: x => x.DistributionSessionId,
                        principalTable: "DistributionSessions",
                        principalColumn: "DistributionSessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DistributionSessionRequests_ReliefRequests_ReliefRequestId",
                        column: x => x.ReliefRequestId,
                        principalTable: "ReliefRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReliefFulfillments",
                columns: table => new
                {
                    ReliefFulfillmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributionSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Mode = table.Column<string>(type: "text", nullable: false),
                    ProofImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RecipientPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    WaveNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReliefFulfillments", x => x.ReliefFulfillmentId);
                    table.ForeignKey(
                        name: "FK_ReliefFulfillments_DistributionSessions_DistributionSession~",
                        column: x => x.DistributionSessionId,
                        principalTable: "DistributionSessions",
                        principalColumn: "DistributionSessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReliefFulfillments_ReliefRequests_ReliefRequestId",
                        column: x => x.ReliefRequestId,
                        principalTable: "ReliefRequests",
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
                    Note = table.Column<string>(type: "text", nullable: true),
                    PeopleCount = table.Column<int>(type: "integer", nullable: false),
                    UrgencyLevel = table.Column<string>(type: "text", nullable: false)
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
                name: "ReliefFulfillmentItems",
                columns: table => new
                {
                    ReliefFulfillmentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefFulfillmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualDeliveredQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    NeedCategory = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PlannedQuantity = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReliefFulfillmentItems", x => x.ReliefFulfillmentItemId);
                    table.ForeignKey(
                        name: "FK_ReliefFulfillmentItems_ReliefFulfillments_ReliefFulfillment~",
                        column: x => x.ReliefFulfillmentId,
                        principalTable: "ReliefFulfillments",
                        principalColumn: "ReliefFulfillmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReliefFulfillmentItems_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DistributionSessionItems_DistributionSessionId_SupplyItemId~",
                table: "DistributionSessionItems",
                columns: new[] { "DistributionSessionId", "SupplyItemId", "SupplyAllocationItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DistributionSessionItems_SupplyAllocationItemId",
                table: "DistributionSessionItems",
                column: "SupplyAllocationItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionSessionItems_SupplyItemId",
                table: "DistributionSessionItems",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionSessionRequests_ReliefRequestId",
                table: "DistributionSessionRequests",
                column: "ReliefRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionSessions_CampaignId_Status",
                table: "DistributionSessions",
                columns: new[] { "CampaignId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DistributionSessions_ReliefStationId_Status",
                table: "DistributionSessions",
                columns: new[] { "ReliefStationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ReliefFulfillmentItems_ReliefFulfillmentId",
                table: "ReliefFulfillmentItems",
                column: "ReliefFulfillmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefFulfillmentItems_SupplyItemId",
                table: "ReliefFulfillmentItems",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefFulfillments_DistributionSessionId",
                table: "ReliefFulfillments",
                column: "DistributionSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefFulfillments_ReliefRequestId_DeliveredAt",
                table: "ReliefFulfillments",
                columns: new[] { "ReliefRequestId", "DeliveredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReliefNeedItems_ReliefRequestId",
                table: "ReliefNeedItems",
                column: "ReliefRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefRequests_AssignedReliefStationId",
                table: "ReliefRequests",
                column: "AssignedReliefStationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefRequests_CampaignId",
                table: "ReliefRequests",
                column: "CampaignId");
        }
    }
}
