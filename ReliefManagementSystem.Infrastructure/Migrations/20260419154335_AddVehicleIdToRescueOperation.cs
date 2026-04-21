using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleIdToRescueOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VehicleId",
                table: "RescueOperations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperations_VehicleId",
                table: "RescueOperations",
                column: "VehicleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RescueOperations_Vehicles_VehicleId",
                table: "RescueOperations",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RescueOperations_Vehicles_VehicleId",
                table: "RescueOperations");

            migrationBuilder.DropIndex(
                name: "IX_RescueOperations_VehicleId",
                table: "RescueOperations");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "RescueOperations");
        }
    }
}
