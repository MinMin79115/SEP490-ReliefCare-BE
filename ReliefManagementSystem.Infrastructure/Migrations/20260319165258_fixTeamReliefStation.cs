using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixTeamReliefStation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ReliefStationTeams");

            migrationBuilder.DropColumn(
                name: "TransferredAt",
                table: "ReliefStationTeams");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ReliefStations");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "ReliefStations",
                newName: "ReliefStationStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReliefStationStatus",
                table: "ReliefStations",
                newName: "Status");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ReliefStationTeams",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransferredAt",
                table: "ReliefStationTeams",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ReliefStations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
