using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addReliefStationIdIntoManagerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReliefStationId",
                table: "ManagerProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplyTransfers",
                columns: table => new
                {
                    SupplyTransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransferCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationStationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ShippedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyTransfers", x => x.SupplyTransferId);
                    table.ForeignKey(
                        name: "FK_SupplyTransfers_AspNetUsers_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SupplyTransfers_AspNetUsers_RequestedBy",
                        column: x => x.RequestedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplyTransfers_ReliefStations_DestinationStationId",
                        column: x => x.DestinationStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplyTransfers_ReliefStations_SourceStationId",
                        column: x => x.SourceStationId,
                        principalTable: "ReliefStations",
                        principalColumn: "ReliefStationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupplyTransferItems",
                columns: table => new
                {
                    SupplyTransferItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyTransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplyItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedQuantity = table.Column<int>(type: "integer", nullable: false),
                    ActualQuantity = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplyTransferItems", x => x.SupplyTransferItemId);
                    table.ForeignKey(
                        name: "FK_SupplyTransferItems_SupplyItems_SupplyItemId",
                        column: x => x.SupplyItemId,
                        principalTable: "SupplyItems",
                        principalColumn: "SupplyItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplyTransferItems_SupplyTransfers_SupplyTransferId",
                        column: x => x.SupplyTransferId,
                        principalTable: "SupplyTransfers",
                        principalColumn: "SupplyTransferId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManagerProfiles_ReliefStationId",
                table: "ManagerProfiles",
                column: "ReliefStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId_IsRead",
                table: "Notifications",
                columns: new[] { "RecipientId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransferItems_SupplyItemId",
                table: "SupplyTransferItems",
                column: "SupplyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransferItems_SupplyTransferId",
                table: "SupplyTransferItems",
                column: "SupplyTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransfers_ApprovedBy",
                table: "SupplyTransfers",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransfers_DestinationStationId",
                table: "SupplyTransfers",
                column: "DestinationStationId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransfers_RequestedBy",
                table: "SupplyTransfers",
                column: "RequestedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransfers_SourceStationId",
                table: "SupplyTransfers",
                column: "SourceStationId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplyTransfers_TransferCode",
                table: "SupplyTransfers",
                column: "TransferCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ManagerProfiles_ReliefStations_ReliefStationId",
                table: "ManagerProfiles",
                column: "ReliefStationId",
                principalTable: "ReliefStations",
                principalColumn: "ReliefStationId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManagerProfiles_ReliefStations_ReliefStationId",
                table: "ManagerProfiles");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "SupplyTransferItems");

            migrationBuilder.DropTable(
                name: "SupplyTransfers");

            migrationBuilder.DropIndex(
                name: "IX_ManagerProfiles_ReliefStationId",
                table: "ManagerProfiles");

            migrationBuilder.DropColumn(
                name: "ReliefStationId",
                table: "ManagerProfiles");
        }
    }
}
