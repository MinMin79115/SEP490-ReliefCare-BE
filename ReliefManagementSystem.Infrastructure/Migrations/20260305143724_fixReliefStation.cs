using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixReliefStation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManagerProfiles_ReliefStations_ReliefStationId",
                table: "ManagerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ManagerProfiles_ReliefStationId",
                table: "ManagerProfiles");

            migrationBuilder.DropColumn(
                name: "ReliefStationId",
                table: "ManagerProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReliefStationId",
                table: "ManagerProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManagerProfiles_ReliefStationId",
                table: "ManagerProfiles",
                column: "ReliefStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ManagerProfiles_ReliefStations_ReliefStationId",
                table: "ManagerProfiles",
                column: "ReliefStationId",
                principalTable: "ReliefStations",
                principalColumn: "ReliefStationId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
