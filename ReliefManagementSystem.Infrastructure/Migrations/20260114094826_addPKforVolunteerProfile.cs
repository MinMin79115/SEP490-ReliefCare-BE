using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addPKforVolunteerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerSkills_VolunteerProfiles_VolunteerProfileId",
                table: "VolunteerSkills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VolunteerProfiles",
                table: "VolunteerProfiles");

            migrationBuilder.AddColumn<Guid>(
                name: "VolunteerProfileId",
                table: "VolunteerProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_VolunteerProfiles",
                table: "VolunteerProfiles",
                column: "VolunteerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerProfiles_UserId",
                table: "VolunteerProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerSkills_VolunteerProfiles_VolunteerProfileId",
                table: "VolunteerSkills",
                column: "VolunteerProfileId",
                principalTable: "VolunteerProfiles",
                principalColumn: "VolunteerProfileId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VolunteerSkills_VolunteerProfiles_VolunteerProfileId",
                table: "VolunteerSkills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VolunteerProfiles",
                table: "VolunteerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_VolunteerProfiles_UserId",
                table: "VolunteerProfiles");

            migrationBuilder.DropColumn(
                name: "VolunteerProfileId",
                table: "VolunteerProfiles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VolunteerProfiles",
                table: "VolunteerProfiles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_VolunteerSkills_VolunteerProfiles_VolunteerProfileId",
                table: "VolunteerSkills",
                column: "VolunteerProfileId",
                principalTable: "VolunteerProfiles",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
