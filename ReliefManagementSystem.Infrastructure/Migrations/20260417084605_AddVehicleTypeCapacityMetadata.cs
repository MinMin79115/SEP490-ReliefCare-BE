using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleTypeCapacityMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CapacityKind",
                table: "VehicleTypes",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "CapacityUnit",
                table: "VehicleTypes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "kg");

            migrationBuilder.Sql(@"
                UPDATE ""VehicleTypes""
                SET ""CapacityKind"" = 2,
                    ""CapacityUnit"" = 'people'
                WHERE ""TypeName"" ILIKE '%cứu thương%'
                   OR ""TypeName"" ILIKE '%cuu thuong%'
                   OR ""TypeName"" ILIKE '%khách%'
                   OR ""TypeName"" ILIKE '%khach%'
                   OR ""TypeName"" ILIKE '%ambulance%'
                   OR ""TypeName"" ILIKE '%bus%';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CapacityKind",
                table: "VehicleTypes");

            migrationBuilder.DropColumn(
                name: "CapacityUnit",
                table: "VehicleTypes");
        }
    }
}
