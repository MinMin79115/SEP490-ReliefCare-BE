using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updateTeamProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLeader",
                table: "VolunteerProfiles",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLeader",
                table: "VolunteerProfiles");
        }
    }
}
