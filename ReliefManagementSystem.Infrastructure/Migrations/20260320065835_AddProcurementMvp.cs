using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProcurementMvp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedUnitCost",
                table: "SupplyItems",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProcurementOrders",
                columns: table => new
                {
                    ProcurementOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationInventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderCode = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TotalEstimatedCost = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalActualCost = table.Column<decimal>(type: "numeric", nullable: true),
                    SupplierName = table.Column<string>(type: "text", nullable: true),
                    SupplierContact = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ApprovalNote = table.Column<string>(type: "text", nullable: true),
                    ReceiveNote = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InventoryTransactionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementOrders", x => x.ProcurementOrderId);
                    table.ForeignKey(
                        name: "FK_ProcurementOrders_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcurementOrders_Inventories_DestinationInventoryId",
                        column: x => x.DestinationInventoryId,
                        principalTable: "Inventories",
                        principalColumn: "InventoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcurementOrders_InventoryTransactions_InventoryTransactio~",
                        column: x => x.InventoryTransactionId,
                        principalTable: "InventoryTransactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementOrderItems",
                columns: table => new
                {
                    ProcurementOrderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcurementOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric", nullable: false),
                    ReceivedQuantity = table.Column<int>(type: "integer", nullable: true),
                    ActualUnitCost = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementOrderItems", x => x.ProcurementOrderItemId);
                    table.ForeignKey(
                        name: "FK_ProcurementOrderItems_ProcurementOrders_ProcurementOrderId",
                        column: x => x.ProcurementOrderId,
                        principalTable: "ProcurementOrders",
                        principalColumn: "ProcurementOrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcurementOrderItems_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementOrderItems_ProcurementOrderId",
                table: "ProcurementOrderItems",
                column: "ProcurementOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementOrderItems_SupplyItemId",
                table: "ProcurementOrderItems",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementOrders_CampaignId",
                table: "ProcurementOrders",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementOrders_DestinationInventoryId",
                table: "ProcurementOrders",
                column: "DestinationInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementOrders_InventoryTransactionId",
                table: "ProcurementOrders",
                column: "InventoryTransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcurementOrderItems");

            migrationBuilder.DropTable(
                name: "ProcurementOrders");

            migrationBuilder.DropColumn(
                name: "EstimatedUnitCost",
                table: "SupplyItems");
        }
    }
}
