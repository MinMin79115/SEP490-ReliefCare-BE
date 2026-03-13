using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixRescueRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "RescueRequests",
                newName: "PriorityPoint");

            migrationBuilder.AddColumn<int>(
                name: "RescuePriorityLevel",
                table: "RescueRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RescuePriorityLevel",
                table: "RescueRequests");

            migrationBuilder.RenameColumn(
                name: "PriorityPoint",
                table: "RescueRequests",
                newName: "Priority");
        }
    }
}
