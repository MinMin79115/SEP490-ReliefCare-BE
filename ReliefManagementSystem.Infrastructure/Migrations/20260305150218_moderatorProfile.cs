using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class moderatorProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReliefStations_AspNetUsers_ManagerId",
                table: "ReliefStations");

            migrationBuilder.DropIndex(
                name: "IX_ReliefStations_ManagerId",
                table: "ReliefStations");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "ReliefStations");

            migrationBuilder.DropColumn(
                name: "AssignedArea",
                table: "ModeratorProfiles");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ReliefStations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStationHead",
                table: "ModeratorProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ReliefStationId",
                table: "ModeratorProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModeratorProfiles_ReliefStationId",
                table: "ModeratorProfiles",
                column: "ReliefStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ModeratorProfiles_ReliefStations_ReliefStationId",
                table: "ModeratorProfiles",
                column: "ReliefStationId",
                principalTable: "ReliefStations",
                principalColumn: "ReliefStationId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModeratorProfiles_ReliefStations_ReliefStationId",
                table: "ModeratorProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ModeratorProfiles_ReliefStationId",
                table: "ModeratorProfiles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ReliefStations");

            migrationBuilder.DropColumn(
                name: "IsStationHead",
                table: "ModeratorProfiles");

            migrationBuilder.DropColumn(
                name: "ReliefStationId",
                table: "ModeratorProfiles");

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "ReliefStations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedArea",
                table: "ModeratorProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReliefStations_ManagerId",
                table: "ReliefStations",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReliefStations_AspNetUsers_ManagerId",
                table: "ReliefStations",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
