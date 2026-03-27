using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class relationshipCampaignAndRescuseRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CampaignId",
                table: "RescueRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RescueRequests_CampaignId",
                table: "RescueRequests",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_RescueRequests_Campaigns_CampaignId",
                table: "RescueRequests",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RescueRequests_Campaigns_CampaignId",
                table: "RescueRequests");

            migrationBuilder.DropIndex(
                name: "IX_RescueRequests_CampaignId",
                table: "RescueRequests");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "RescueRequests");
        }
    }
}
