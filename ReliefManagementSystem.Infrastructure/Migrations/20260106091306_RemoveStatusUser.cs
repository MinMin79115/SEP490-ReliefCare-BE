using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStatusUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
