using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplyTransferDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupplyTransferDocuments",
                columns: table => new
                {
                    SupplyTransferDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyTransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyTransferDocuments", x => x.SupplyTransferDocumentId);
                    table.ForeignKey(
                        name: "FK_SupplyTransferDocuments_SupplyTransfers_SupplyTransferId",
                        column: x => x.SupplyTransferId,
                        principalTable: "SupplyTransfers",
                        principalColumn: "SupplyTransferId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransferDocuments_SupplyTransferId_DocumentType",
                table: "SupplyTransferDocuments",
                columns: new[] { "SupplyTransferId", "DocumentType" },
                unique: true,
                filter: "\"IsCurrent\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransferDocuments_SupplyTransferId_DocumentType_Versi~",
                table: "SupplyTransferDocuments",
                columns: new[] { "SupplyTransferId", "DocumentType", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplyTransferDocuments");
        }
    }
}
