using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removeCreateByStation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campaigns_ReliefStations_CreatedByStationId",
                table: "Campaigns");

            migrationBuilder.DropIndex(
                name: "IX_Campaigns_CreatedByStationId",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "CreatedByStationId",
                table: "Campaigns");

            migrationBuilder.AddColumn<Guid>(
                name: "CampaignId",
                table: "ReliefStations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReliefStations_CampaignId",
                table: "ReliefStations",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReliefStations_Campaigns_CampaignId",
                table: "ReliefStations",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReliefStations_Campaigns_CampaignId",
                table: "ReliefStations");

            migrationBuilder.DropIndex(
                name: "IX_ReliefStations_CampaignId",
                table: "ReliefStations");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "ReliefStations");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByStationId",
                table: "Campaigns",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CreatedByStationId",
                table: "Campaigns",
                column: "CreatedByStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_ReliefStations_CreatedByStationId",
                table: "Campaigns",
                column: "CreatedByStationId",
                principalTable: "ReliefStations",
                principalColumn: "ReliefStationId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
