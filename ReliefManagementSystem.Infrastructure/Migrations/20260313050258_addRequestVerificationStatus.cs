using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addRequestVerificationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Result",
                table: "RequestVerifications");

            migrationBuilder.Sql(
                @"ALTER TABLE ""RequestVerifications""
                ALTER COLUMN ""Method"" TYPE integer
                USING ""Method""::integer;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"ALTER TABLE ""RequestVerifications""
          ALTER COLUMN ""Method"" TYPE text
          USING ""Method""::text;");

            migrationBuilder.AddColumn<string>(
                name: "Result",
                table: "RequestVerifications",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
