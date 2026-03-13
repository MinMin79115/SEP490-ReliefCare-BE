using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addStatusRescuOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 ALTER TABLE "RescueOperations"
                                 ALTER COLUMN "Status"
                                 TYPE integer
                                 USING CASE
                                     WHEN "Status" = 'Pending' THEN 0
                                     WHEN "Status" = 'Assigned' THEN 1
                                     WHEN "Status" = 'EnRoute' THEN 2
                                     WHEN "Status" = 'Rescuing' THEN 3
                                     WHEN "Status" = 'Completed' THEN 4
                                     ELSE 0
                                 END;
                                 """);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "RescueOperations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "RescueOperations",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "RescueOperations",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
