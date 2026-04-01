using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReliefFlowPhase1To4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "ReliefRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedReliefStationId",
                table: "ReliefRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "ReliefRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DistributionSessions",
                columns: table => new
                {
                    DistributionSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Mode = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ScheduledStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScheduledEndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LocationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    RadiusMeters = table.Column<double>(type: "double precision", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                name: "DistributionSessionItems",
                columns: table => new
                {
                    DistributionSessionItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributionSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyAllocationItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReservedQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    DeliveredQuantity = table.Column<decimal>(type: "numeric", nullable: false)
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
                    ReliefRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistributionSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WaveNumber = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RecipientPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DeliveryNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ProofImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                name: "ReliefFulfillmentItems",
                columns: table => new
                {
                    ReliefFulfillmentItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefFulfillmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    NeedCategory = table.Column<string>(type: "text", nullable: true),
                    PlannedQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    ActualDeliveredQuantity = table.Column<decimal>(type: "numeric", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
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
                name: "IX_ReliefRequests_AssignedReliefStationId",
                table: "ReliefRequests",
                column: "AssignedReliefStationId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ReliefRequests_ReliefStations_AssignedReliefStationId",
                table: "ReliefRequests",
                column: "AssignedReliefStationId",
                principalTable: "ReliefStations",
                principalColumn: "ReliefStationId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReliefRequests_ReliefStations_AssignedReliefStationId",
                table: "ReliefRequests");

            migrationBuilder.DropTable(
                name: "DistributionSessionItems");

            migrationBuilder.DropTable(
                name: "DistributionSessionRequests");

            migrationBuilder.DropTable(
                name: "ReliefFulfillmentItems");

            migrationBuilder.DropTable(
                name: "ReliefFulfillments");

            migrationBuilder.DropTable(
                name: "DistributionSessions");

            migrationBuilder.DropIndex(
                name: "IX_ReliefRequests_AssignedReliefStationId",
                table: "ReliefRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "ReliefRequests");

            migrationBuilder.DropColumn(
                name: "AssignedReliefStationId",
                table: "ReliefRequests");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "ReliefRequests");
        }
    }
}
