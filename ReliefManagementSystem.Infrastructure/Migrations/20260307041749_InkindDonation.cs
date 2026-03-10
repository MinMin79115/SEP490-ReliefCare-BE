using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InkindDonation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DriverUserId",
                table: "SupplyTransfers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleId",
                table: "SupplyTransfers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InventoryTransactionId",
                table: "SupplyAllocations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Reason",
                table: "InventoryTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SupplyTransferId",
                table: "InventoryTransactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "CampaignTasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "CampaignTaskItems",
                columns: table => new
                {
                    CampaignTaskItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyAllocationItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityAssigned = table.Column<int>(type: "integer", nullable: false),
                    QuantityDelivered = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignTaskItems", x => x.CampaignTaskItemId);
                    table.ForeignKey(
                        name: "FK_CampaignTaskItems_CampaignTasks_CampaignTaskId",
                        column: x => x.CampaignTaskId,
                        principalTable: "CampaignTasks",
                        principalColumn: "CampaignTaskId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignTaskItems_SupplyAllocationItems_SupplyAllocationIte~",
                        column: x => x.SupplyAllocationItemId,
                        principalTable: "SupplyAllocationItems",
                        principalColumn: "AllocationItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InKindDonations",
                columns: table => new
                {
                    InKindDonationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DonorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: false),
                    DonorName = table.Column<string>(type: "text", nullable: true),
                    DonorContact = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    DonatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    InventoryTransactionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InKindDonations", x => x.InKindDonationId);
                    table.ForeignKey(
                        name: "FK_InKindDonations_AspNetUsers_DonorUserId",
                        column: x => x.DonorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InKindDonations_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InKindDonations_InventoryTransactions_InventoryTransactionId",
                        column: x => x.InventoryTransactionId,
                        principalTable: "InventoryTransactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InKindDonations_ReliefStations_ReliefStationId",
                        column: x => x.ReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberTaskItems",
                columns: table => new
                {
                    MemberTaskItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignTaskItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityAssigned = table.Column<int>(type: "integer", nullable: false),
                    QuantityDelivered = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberTaskItems", x => x.MemberTaskItemId);
                    table.ForeignKey(
                        name: "FK_MemberTaskItems_CampaignTaskItems_CampaignTaskItemId",
                        column: x => x.CampaignTaskItemId,
                        principalTable: "CampaignTaskItems",
                        principalColumn: "CampaignTaskItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberTaskItems_MemberTasks_MemberTaskId",
                        column: x => x.MemberTaskId,
                        principalTable: "MemberTasks",
                        principalColumn: "MemberTaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InKindDonationDetails",
                columns: table => new
                {
                    InKindDonationDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    InKindDonationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InKindDonationDetails", x => x.InKindDonationDetailId);
                    table.ForeignKey(
                        name: "FK_InKindDonationDetails_InKindDonations_InKindDonationId",
                        column: x => x.InKindDonationId,
                        principalTable: "InKindDonations",
                        principalColumn: "InKindDonationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InKindDonationDetails_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransfers_DriverUserId",
                table: "SupplyTransfers",
                column: "DriverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransfers_VehicleId",
                table: "SupplyTransfers",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyAllocations_InventoryTransactionId",
                table: "SupplyAllocations",
                column: "InventoryTransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_SupplyTransferId",
                table: "InventoryTransactions",
                column: "SupplyTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTaskItems_CampaignTaskId",
                table: "CampaignTaskItems",
                column: "CampaignTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignTaskItems_SupplyAllocationItemId",
                table: "CampaignTaskItems",
                column: "SupplyAllocationItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InKindDonationDetails_InKindDonationId",
                table: "InKindDonationDetails",
                column: "InKindDonationId");

            migrationBuilder.CreateIndex(
                name: "IX_InKindDonationDetails_SupplyItemId",
                table: "InKindDonationDetails",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InKindDonations_CampaignId",
                table: "InKindDonations",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_InKindDonations_DonorUserId",
                table: "InKindDonations",
                column: "DonorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InKindDonations_InventoryTransactionId",
                table: "InKindDonations",
                column: "InventoryTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_InKindDonations_ReliefStationId",
                table: "InKindDonations",
                column: "ReliefStationId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberTaskItems_CampaignTaskItemId",
                table: "MemberTaskItems",
                column: "CampaignTaskItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberTaskItems_MemberTaskId",
                table: "MemberTaskItems",
                column: "MemberTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_SupplyTransfers_SupplyTransferId",
                table: "InventoryTransactions",
                column: "SupplyTransferId",
                principalTable: "SupplyTransfers",
                principalColumn: "SupplyTransferId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyAllocations_InventoryTransactions_InventoryTransactio~",
                table: "SupplyAllocations",
                column: "InventoryTransactionId",
                principalTable: "InventoryTransactions",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyTransfers_AspNetUsers_DriverUserId",
                table: "SupplyTransfers",
                column: "DriverUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplyTransfers_Vehicles_VehicleId",
                table: "SupplyTransfers",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_SupplyTransfers_SupplyTransferId",
                table: "InventoryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplyAllocations_InventoryTransactions_InventoryTransactio~",
                table: "SupplyAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplyTransfers_AspNetUsers_DriverUserId",
                table: "SupplyTransfers");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplyTransfers_Vehicles_VehicleId",
                table: "SupplyTransfers");

            migrationBuilder.DropTable(
                name: "InKindDonationDetails");

            migrationBuilder.DropTable(
                name: "MemberTaskItems");

            migrationBuilder.DropTable(
                name: "InKindDonations");

            migrationBuilder.DropTable(
                name: "CampaignTaskItems");

            migrationBuilder.DropIndex(
                name: "IX_SupplyTransfers_DriverUserId",
                table: "SupplyTransfers");

            migrationBuilder.DropIndex(
                name: "IX_SupplyTransfers_VehicleId",
                table: "SupplyTransfers");

            migrationBuilder.DropIndex(
                name: "IX_SupplyAllocations_InventoryTransactionId",
                table: "SupplyAllocations");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactions_SupplyTransferId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "DriverUserId",
                table: "SupplyTransfers");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "SupplyTransfers");

            migrationBuilder.DropColumn(
                name: "InventoryTransactionId",
                table: "SupplyAllocations");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "SupplyTransferId",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CampaignTasks");
        }
    }
}
