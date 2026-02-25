using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixReliefSation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ReliefStations");

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "ReliefStations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "ReliefStations");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ReliefStations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
