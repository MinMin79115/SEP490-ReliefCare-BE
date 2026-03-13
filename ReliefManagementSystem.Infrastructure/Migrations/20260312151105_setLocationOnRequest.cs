using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class setLocationOnRequest : Migration
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

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "Requests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_LocationId",
                table: "Requests",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Locations_LocationId",
                table: "Requests",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Locations_LocationId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_LocationId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Requests");

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
