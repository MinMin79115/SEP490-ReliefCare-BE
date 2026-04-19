using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignOperationalInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampaignInventories",
                columns: table => new
                {
                    CampaignInventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignInventories", x => x.CampaignInventoryId);
                    table.ForeignKey(
                        name: "FK_CampaignInventories_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignInventoryStocks",
                columns: table => new
                {
                    CampaignInventoryStockId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignInventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentQuantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignInventoryStocks", x => x.CampaignInventoryStockId);
                    table.CheckConstraint("CK_CampaignInventoryStocks_CurrentQuantity_NonNegative", "\"CurrentQuantity\" >= 0");
                    table.ForeignKey(
                        name: "FK_CampaignInventoryStocks_CampaignInventories_CampaignInvento~",
                        column: x => x.CampaignInventoryId,
                        principalTable: "CampaignInventories",
                        principalColumn: "CampaignInventoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignInventoryStocks_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CampaignInventoryTransactions",
                columns: table => new
                {
                    CampaignInventoryTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignInventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    SupplyAllocationId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignInventoryTransactions", x => x.CampaignInventoryTransactionId);
                    table.ForeignKey(
                        name: "FK_CampaignInventoryTransactions_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CampaignInventoryTransactions_CampaignInventories_CampaignI~",
                        column: x => x.CampaignInventoryId,
                        principalTable: "CampaignInventories",
                        principalColumn: "CampaignInventoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignInventoryTransactions_SupplyAllocations_SupplyAlloc~",
                        column: x => x.SupplyAllocationId,
                        principalTable: "SupplyAllocations",
                        principalColumn: "AllocationId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CampaignInventoryTransactionItems",
                columns: table => new
                {
                    CampaignInventoryTransactionItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignInventoryTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignInventoryTransactionItems", x => x.CampaignInventoryTransactionItemId);
                    table.ForeignKey(
                        name: "FK_CampaignInventoryTransactionItems_CampaignInventoryTransact~",
                        column: x => x.CampaignInventoryTransactionId,
                        principalTable: "CampaignInventoryTransactions",
                        principalColumn: "CampaignInventoryTransactionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CampaignInventoryTransactionItems_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventories_CampaignId",
                table: "CampaignInventories",
                column: "CampaignId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryStocks_CampaignInventoryId_SupplyItemId",
                table: "CampaignInventoryStocks",
                columns: new[] { "CampaignInventoryId", "SupplyItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryStocks_SupplyItemId",
                table: "CampaignInventoryStocks",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactionItems_CampaignInventoryTransact~",
                table: "CampaignInventoryTransactionItems",
                column: "CampaignInventoryTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactionItems_SupplyItemId",
                table: "CampaignInventoryTransactionItems",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactions_CampaignInventoryId",
                table: "CampaignInventoryTransactions",
                column: "CampaignInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactions_CreatedBy",
                table: "CampaignInventoryTransactions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignInventoryTransactions_SupplyAllocationId",
                table: "CampaignInventoryTransactions",
                column: "SupplyAllocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignInventoryStocks");

            migrationBuilder.DropTable(
                name: "CampaignInventoryTransactionItems");

            migrationBuilder.DropTable(
                name: "CampaignInventoryTransactions");

            migrationBuilder.DropTable(
                name: "CampaignInventories");
        }
    }
}
