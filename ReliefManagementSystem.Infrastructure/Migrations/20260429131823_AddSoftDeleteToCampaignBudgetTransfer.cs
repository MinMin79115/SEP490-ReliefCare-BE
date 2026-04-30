using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToCampaignBudgetTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "CampaignBudgetTransfers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledByUserId",
                table: "CampaignBudgetTransfers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CampaignBudgetTransfers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignBudgetTransfers_CancelledByUserId",
                table: "CampaignBudgetTransfers",
                column: "CancelledByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignBudgetTransfers_AspNetUsers_CancelledByUserId",
                table: "CampaignBudgetTransfers",
                column: "CancelledByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignBudgetTransfers_AspNetUsers_CancelledByUserId",
                table: "CampaignBudgetTransfers");

            migrationBuilder.DropIndex(
                name: "IX_CampaignBudgetTransfers_CancelledByUserId",
                table: "CampaignBudgetTransfers");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "CampaignBudgetTransfers");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "CampaignBudgetTransfers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CampaignBudgetTransfers");
        }
    }
}
