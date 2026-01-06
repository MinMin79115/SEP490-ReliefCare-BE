using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixVunteerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerProfiles_AspNetUsers_UserId1",
                table: "VolunteerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_VolunteerProfiles_UserId1",
                table: "VolunteerProfiles");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "VolunteerProfiles");

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerProfiles_AspNetUsers_UserId",
                table: "VolunteerProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerProfiles_AspNetUsers_UserId",
                table: "VolunteerProfiles");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "VolunteerProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerProfiles_UserId1",
                table: "VolunteerProfiles",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerProfiles_AspNetUsers_UserId1",
                table: "VolunteerProfiles",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
