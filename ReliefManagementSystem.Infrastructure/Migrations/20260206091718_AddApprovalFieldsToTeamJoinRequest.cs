using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalFieldsToTeamJoinRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestedRole",
                table: "TeamJoinRequests");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "TeamJoinRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "TeamJoinRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TeamJoinRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "TeamJoinRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectedBy",
                table: "TeamJoinRequests",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "TeamJoinRequests");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "TeamJoinRequests");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TeamJoinRequests");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "TeamJoinRequests");

            migrationBuilder.DropColumn(
                name: "RejectedBy",
                table: "TeamJoinRequests");

            migrationBuilder.AddColumn<int>(
                name: "RequestedRole",
                table: "TeamJoinRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
