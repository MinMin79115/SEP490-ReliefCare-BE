using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRescueOperationSupplyFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RescueOperationSupplies",
                columns: table => new
                {
                    RescueOperationSupplyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RescueOperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceInventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    InventoryTransactionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RescueOperationSupplies", x => x.RescueOperationSupplyId);
                    table.ForeignKey(
                        name: "FK_RescueOperationSupplies_Inventories_SourceInventoryId",
                        column: x => x.SourceInventoryId,
                        principalTable: "Inventories",
                        principalColumn: "InventoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RescueOperationSupplies_InventoryTransactions_InventoryTran~",
                        column: x => x.InventoryTransactionId,
                        principalTable: "InventoryTransactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RescueOperationSupplies_RescueOperations_RescueOperationId",
                        column: x => x.RescueOperationId,
                        principalTable: "RescueOperations",
                        principalColumn: "RescueOperationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RescueOperationSupplies_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperationSupplies_InventoryTransactionId",
                table: "RescueOperationSupplies",
                column: "InventoryTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperationSupplies_RescueOperationId",
                table: "RescueOperationSupplies",
                column: "RescueOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperationSupplies_SourceInventoryId",
                table: "RescueOperationSupplies",
                column: "SourceInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RescueOperationSupplies_SupplyItemId",
                table: "RescueOperationSupplies",
                column: "SupplyItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RescueOperationSupplies");
        }
    }
}
