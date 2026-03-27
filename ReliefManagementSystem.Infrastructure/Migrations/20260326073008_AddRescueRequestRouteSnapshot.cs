using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRescueRequestRouteSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "StationToRequestDistanceKm",
                table: "RescueRequests",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StationToRequestDistanceMeters",
                table: "RescueRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StationToRequestDurationMinutes",
                table: "RescueRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StationToRequestDurationSeconds",
                table: "RescueRequests",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StationToRequestDistanceKm",
                table: "RescueRequests");

            migrationBuilder.DropColumn(
                name: "StationToRequestDistanceMeters",
                table: "RescueRequests");

            migrationBuilder.DropColumn(
                name: "StationToRequestDurationMinutes",
                table: "RescueRequests");

            migrationBuilder.DropColumn(
                name: "StationToRequestDurationSeconds",
                table: "RescueRequests");
        }
    }
}
