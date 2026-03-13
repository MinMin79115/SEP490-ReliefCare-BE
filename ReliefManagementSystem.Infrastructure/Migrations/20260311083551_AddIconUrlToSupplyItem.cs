using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIconUrlToSupplyItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReliefStations_ReliefStations_ParentStationReliefStationId",
                table: "ReliefStations");

            migrationBuilder.DropIndex(
                name: "IX_ReliefStations_ParentStationReliefStationId",
                table: "ReliefStations");

            migrationBuilder.DropColumn(
                name: "ParentReliefStationId",
                table: "ReliefStations");

            migrationBuilder.DropColumn(
                name: "ParentStationReliefStationId",
                table: "ReliefStations");

            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "SupplyItems",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "SupplyItems");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentReliefStationId",
                table: "ReliefStations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentStationReliefStationId",
                table: "ReliefStations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReliefStations_ParentStationReliefStationId",
                table: "ReliefStations",
                column: "ParentStationReliefStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReliefStations_ReliefStations_ParentStationReliefStationId",
                table: "ReliefStations",
                column: "ParentStationReliefStationId",
                principalTable: "ReliefStations",
                principalColumn: "ReliefStationId");
        }
    }
}
