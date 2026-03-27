using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addRescueWeatherCondition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WeatherCondition",
                table: "RescueRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WeatherObservedAt",
                table: "RescueRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WeatherPrecipMm",
                table: "RescueRequests",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeatherRiskLevel",
                table: "RescueRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeatherRiskScore",
                table: "RescueRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WeatherTempC",
                table: "RescueRequests",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WeatherVisibilityKm",
                table: "RescueRequests",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WeatherWindKph",
                table: "RescueRequests",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeatherCondition",
                table: "RescueRequests");

            migrationBuilder.DropColumn(
                name: "WeatherObservedAt",
                table: "RescueRequests");

            migrationBuilder.DropColumn(
                name: "WeatherPrecipMm",
                table: "RescueRequests");

            migrationBuilder.DropColumn(
                name: "WeatherRiskLevel",
                table: "RescueRequests");

            migrationBuilder.DropColumn(
                name: "WeatherRiskScore",
                table: "RescueRequests");

            migrationBuilder.DropColumn(
                name: "WeatherTempC",
                table: "RescueRequests");

            migrationBuilder.DropColumn(
                name: "WeatherVisibilityKm",
                table: "RescueRequests");

            migrationBuilder.DropColumn(
                name: "WeatherWindKph",
                table: "RescueRequests");
        }
    }
}
