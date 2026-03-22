using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixCreatedByTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_AspNetUsers_ModeratorId",
                table: "Teams");

            migrationBuilder.RenameColumn(
                name: "ModeratorId",
                table: "Teams",
                newName: "CreateBy");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_ModeratorId",
                table: "Teams",
                newName: "IX_Teams_CreateBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_AspNetUsers_CreateBy",
                table: "Teams",
                column: "CreateBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_AspNetUsers_CreateBy",
                table: "Teams");

            migrationBuilder.RenameColumn(
                name: "CreateBy",
                table: "Teams",
                newName: "ModeratorId");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_CreateBy",
                table: "Teams",
                newName: "IX_Teams_ModeratorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_AspNetUsers_ModeratorId",
                table: "Teams",
                column: "ModeratorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
