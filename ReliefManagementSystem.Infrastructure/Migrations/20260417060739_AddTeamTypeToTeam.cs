using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamTypeToTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamUsed",
                table: "Vehicles");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReliefStationId",
                table: "Vehicles",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "Vehicles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamType",
                table: "Teams",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_TeamId",
                table: "Vehicles",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Teams_TeamId",
                table: "Vehicles",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Teams_TeamId",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_TeamId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "TeamType",
                table: "Teams");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReliefStationId",
                table: "Vehicles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamUsed",
                table: "Vehicles",
                type: "text",
                nullable: true);
        }
    }
}
