using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class allInRescue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RescueBatchItems");

            migrationBuilder.DropTable(
                name: "TeamTrackingPoints");

            migrationBuilder.DropTable(
                name: "RescueBatches");
        }
    }
}
