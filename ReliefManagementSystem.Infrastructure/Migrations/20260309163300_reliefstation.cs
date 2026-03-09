using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class reliefstation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReliefStations_Campaigns_CampaignId",
                table: "ReliefStations");

            migrationBuilder.DropForeignKey(
                name: "FK_ReliefStations_ReliefStations_ParentReliefStationId",
                table: "ReliefStations");

            migrationBuilder.DropIndex(
                name: "IX_ReliefStations_ParentReliefStationId",
                table: "ReliefStations");

            migrationBuilder.RenameColumn(
                name: "CampaignId",
                table: "ReliefStations",
                newName: "ParentStationReliefStationId");

            migrationBuilder.RenameIndex(
                name: "IX_ReliefStations_CampaignId",
                table: "ReliefStations",
                newName: "IX_ReliefStations_ParentStationReliefStationId");

            migrationBuilder.AddColumn<string>(
                name: "AddressDetail",
                table: "Campaigns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Campaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CampaignStations",
                columns: table => new
                {
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignStations", x => new { x.CampaignId, x.ReliefStationId });
                    table.ForeignKey(
                        name: "FK_CampaignStations_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignStations_ReliefStations_ReliefStationId",
                        column: x => x.ReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignStations_ReliefStationId",
                table: "CampaignStations",
                column: "ReliefStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReliefStations_ReliefStations_ParentStationReliefStationId",
                table: "ReliefStations",
                column: "ParentStationReliefStationId",
                principalTable: "ReliefStations",
                principalColumn: "ReliefStationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReliefStations_ReliefStations_ParentStationReliefStationId",
                table: "ReliefStations");

            migrationBuilder.DropTable(
                name: "CampaignStations");

            migrationBuilder.DropColumn(
                name: "AddressDetail",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Campaigns");

            migrationBuilder.RenameColumn(
                name: "ParentStationReliefStationId",
                table: "ReliefStations",
                newName: "CampaignId");

            migrationBuilder.RenameIndex(
                name: "IX_ReliefStations_ParentStationReliefStationId",
                table: "ReliefStations",
                newName: "IX_ReliefStations_CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefStations_ParentReliefStationId",
                table: "ReliefStations",
                column: "ParentReliefStationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReliefStations_Campaigns_CampaignId",
                table: "ReliefStations",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReliefStations_ReliefStations_ParentReliefStationId",
                table: "ReliefStations",
                column: "ParentReliefStationId",
                principalTable: "ReliefStations",
                principalColumn: "ReliefStationId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
