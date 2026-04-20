using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCashSupportToReliefDistribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CashSupportAmount",
                table: "ReliefPackageDefinitions",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CashSupportAmount",
                table: "HouseholdDeliveries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CampaignBudgetTransfers",
                columns: table => new
                {
                    CampaignBudgetTransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceCampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetCampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    TransferredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransferredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignBudgetTransfers", x => x.CampaignBudgetTransferId);
                    table.CheckConstraint("CK_CampaignBudgetTransfers_Amount_Positive", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_CampaignBudgetTransfers_AspNetUsers_TransferredByUserId",
                        column: x => x.TransferredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CampaignBudgetTransfers_Campaigns_SourceCampaignId",
                        column: x => x.SourceCampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignBudgetTransfers_Campaigns_TargetCampaignId",
                        column: x => x.TargetCampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignBudgetTransfers_SourceCampaignId_TargetCampaignId_T~",
                table: "CampaignBudgetTransfers",
                columns: new[] { "SourceCampaignId", "TargetCampaignId", "TransferredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignBudgetTransfers_TargetCampaignId",
                table: "CampaignBudgetTransfers",
                column: "TargetCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignBudgetTransfers_TransferredByUserId",
                table: "CampaignBudgetTransfers",
                column: "TransferredByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignBudgetTransfers");

            migrationBuilder.DropColumn(
                name: "CashSupportAmount",
                table: "ReliefPackageDefinitions");

            migrationBuilder.DropColumn(
                name: "CashSupportAmount",
                table: "HouseholdDeliveries");
        }
    }
}
