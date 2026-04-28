using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplyTransferVehicles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.Sql("""
                CREATE EXTENSION IF NOT EXISTS pgcrypto;

                INSERT INTO "SupplyTransferVehicles" (
                    "SupplyTransferVehicleId",
                    "SupplyTransferId",
                    "VehicleId",
                    "DriverUserId",
                    "Status",
                    "AssignedAt",
                    "DepartedAt",
                    "ArrivedAt",
                    "CompletedAt",
                    "Note")
                SELECT
                    gen_random_uuid(),
                    "SupplyTransferId",
                    "VehicleId",
                    "DriverUserId",
                    CASE
                        WHEN "Status" = 'Received' THEN 'Completed'
                        WHEN "Status" = 'Shipping' THEN 'InTransit'
                        ELSE 'Assigned'
                    END,
                    COALESCE("ShippedAt", "RequestedAt"),
                    "ShippedAt",
                    CASE WHEN "Status" = 'Received' THEN "ReceivedAt" ELSE NULL END,
                    CASE WHEN "Status" = 'Received' THEN "ReceivedAt" ELSE NULL END,
                    'Backfilled from legacy SupplyTransfers.VehicleId'
                FROM "SupplyTransfers"
                WHERE "VehicleId" IS NOT NULL;
                """);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplyTransferVehicles");
        }
    }
}
