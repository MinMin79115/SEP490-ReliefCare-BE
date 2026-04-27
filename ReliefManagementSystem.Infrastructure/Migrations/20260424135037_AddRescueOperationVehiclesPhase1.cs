using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRescueOperationVehiclesPhase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperationVehicles_RescueOperationId_VehicleId",
                table: "RescueOperationVehicles",
                columns: new[] { "RescueOperationId", "VehicleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperationVehicles_VehicleId",
                table: "RescueOperationVehicles",
                column: "VehicleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RescueOperationVehicles");
        }
    }
}
