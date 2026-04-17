using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReliefPackageAssemblyWorkflowScaffold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OutputSupplyItemId",
                table: "ReliefPackageDefinitions",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""ReliefPackageDefinitions"" p
                SET ""OutputSupplyItemId"" = i.""SupplyItemId""
                FROM (
                    SELECT DISTINCT ON (""ReliefPackageDefinitionId"")
                        ""ReliefPackageDefinitionId"",
                        ""SupplyItemId""
                    FROM ""ReliefPackageDefinitionItems""
                    ORDER BY ""ReliefPackageDefinitionId"", ""ReliefPackageDefinitionItemId""
                ) i
                WHERE p.""ReliefPackageDefinitionId"" = i.""ReliefPackageDefinitionId""
                  AND p.""OutputSupplyItemId"" IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE ""ReliefPackageDefinitions"" p
                SET ""OutputSupplyItemId"" = s.""SupplyItemId""
                FROM (
                    SELECT ""SupplyItemId""
                    FROM ""SupplyItems""
                    ORDER BY ""SupplyItemId""
                    LIMIT 1
                ) s
                WHERE p.""OutputSupplyItemId"" IS NULL;
            ");

            migrationBuilder.AlterColumn<Guid>(
                name: "OutputSupplyItemId",
                table: "ReliefPackageDefinitions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "ReliefPackageAssemblies",
                columns: table => new
                {
                    ReliefPackageAssemblyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefPackageDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OutputSupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityCreated = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReliefPackageAssemblies", x => x.ReliefPackageAssemblyId);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblies_AspNetUsers_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblies_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblies_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "InventoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblies_ReliefPackageDefinitions_ReliefPack~",
                        column: x => x.ReliefPackageDefinitionId,
                        principalTable: "ReliefPackageDefinitions",
                        principalColumn: "ReliefPackageDefinitionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblies_ReliefStations_ReliefStationId",
                        column: x => x.ReliefStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblies_SupplyItems_OutputSupplyItemId",
                        column: x => x.OutputSupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReliefPackageAssemblyDetails",
                columns: table => new
                {
                    ReliefPackageAssemblyDetailId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReliefPackageAssemblyId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityConsumed = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReliefPackageAssemblyDetails", x => x.ReliefPackageAssemblyDetailId);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblyDetails_ReliefPackageAssemblies_Relief~",
                        column: x => x.ReliefPackageAssemblyId,
                        principalTable: "ReliefPackageAssemblies",
                        principalColumn: "ReliefPackageAssemblyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReliefPackageAssemblyDetails_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageDefinitions_OutputSupplyItemId",
                table: "ReliefPackageDefinitions",
                column: "OutputSupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblies_CampaignId",
                table: "ReliefPackageAssemblies",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblies_CreatedBy",
                table: "ReliefPackageAssemblies",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblies_InventoryId_CreatedAt",
                table: "ReliefPackageAssemblies",
                columns: new[] { "InventoryId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblies_OutputSupplyItemId",
                table: "ReliefPackageAssemblies",
                column: "OutputSupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblies_ReliefPackageDefinitionId_CreatedAt",
                table: "ReliefPackageAssemblies",
                columns: new[] { "ReliefPackageDefinitionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblies_ReliefStationId",
                table: "ReliefPackageAssemblies",
                column: "ReliefStationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblyDetails_ReliefPackageAssemblyId_Supply~",
                table: "ReliefPackageAssemblyDetails",
                columns: new[] { "ReliefPackageAssemblyId", "SupplyItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReliefPackageAssemblyDetails_SupplyItemId",
                table: "ReliefPackageAssemblyDetails",
                column: "SupplyItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReliefPackageDefinitions_SupplyItems_OutputSupplyItemId",
                table: "ReliefPackageDefinitions",
                column: "OutputSupplyItemId",
                principalTable: "SupplyItems",
                principalColumn: "SupplyItemId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReliefPackageDefinitions_SupplyItems_OutputSupplyItemId",
                table: "ReliefPackageDefinitions");

            migrationBuilder.DropTable(
                name: "ReliefPackageAssemblyDetails");

            migrationBuilder.DropTable(
                name: "ReliefPackageAssemblies");

            migrationBuilder.DropIndex(
                name: "IX_ReliefPackageDefinitions_OutputSupplyItemId",
                table: "ReliefPackageDefinitions");

            migrationBuilder.DropColumn(
                name: "OutputSupplyItemId",
                table: "ReliefPackageDefinitions");
        }
    }
}
