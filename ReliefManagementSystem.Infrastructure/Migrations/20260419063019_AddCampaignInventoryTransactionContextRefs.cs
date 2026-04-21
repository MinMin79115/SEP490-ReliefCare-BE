using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignInventoryTransactionContextRefs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CampaignTeamId",
                table: "CampaignInventoryTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DistributionPointId",
                table: "CampaignInventoryTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HouseholdDeliveryId",
                table: "CampaignInventoryTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReliefPackageDefinitionId",
                table: "CampaignInventoryTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactions_CampaignTeamId",
                table: "CampaignInventoryTransactions",
                column: "CampaignTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactions_DistributionPointId",
                table: "CampaignInventoryTransactions",
                column: "DistributionPointId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactions_HouseholdDeliveryId",
                table: "CampaignInventoryTransactions",
                column: "HouseholdDeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactions_ReliefPackageDefinitionId",
                table: "CampaignInventoryTransactions",
                column: "ReliefPackageDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CampaignInventoryTransactions_CampaignTeamId",
                table: "CampaignInventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CampaignInventoryTransactions_DistributionPointId",
                table: "CampaignInventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CampaignInventoryTransactions_HouseholdDeliveryId",
                table: "CampaignInventoryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CampaignInventoryTransactions_ReliefPackageDefinitionId",
                table: "CampaignInventoryTransactions");

            migrationBuilder.DropColumn(
                name: "CampaignTeamId",
                table: "CampaignInventoryTransactions");

            migrationBuilder.DropColumn(
                name: "DistributionPointId",
                table: "CampaignInventoryTransactions");

            migrationBuilder.DropColumn(
                name: "HouseholdDeliveryId",
                table: "CampaignInventoryTransactions");

            migrationBuilder.DropColumn(
                name: "ReliefPackageDefinitionId",
                table: "CampaignInventoryTransactions");
        }
    }
}
