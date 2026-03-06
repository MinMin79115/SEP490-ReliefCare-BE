using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addSeepayPaymentDetails2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DonationTransactionDetails_DonationTransactions_PaymentTran~",
                table: "DonationTransactionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_DonationTransactions_AspNetUsers_UserId",
                table: "DonationTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_DonationTransactions_Donations_DonationId",
                table: "DonationTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DonationTransactions",
                table: "DonationTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DonationTransactionDetails",
                table: "DonationTransactionDetails");

            migrationBuilder.RenameTable(
                name: "DonationTransactions",
                newName: "PaymentTransactions");

            migrationBuilder.RenameTable(
                name: "DonationTransactionDetails",
                newName: "PaymentTransactionDetails");

            migrationBuilder.RenameIndex(
                name: "IX_DonationTransactions_UserId",
                table: "PaymentTransactions",
                newName: "IX_PaymentTransactions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_DonationTransactions_DonationId",
                table: "PaymentTransactions",
                newName: "IX_PaymentTransactions_DonationId");

            migrationBuilder.RenameIndex(
                name: "IX_DonationTransactionDetails_PaymentTransactionId",
                table: "PaymentTransactionDetails",
                newName: "IX_PaymentTransactionDetails_PaymentTransactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentTransactions",
                table: "PaymentTransactions",
                column: "PaymentTransactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentTransactionDetails",
                table: "PaymentTransactionDetails",
                column: "PaymentTransactionDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactionDetails_PaymentTransactions_PaymentTransa~",
                table: "PaymentTransactionDetails",
                column: "PaymentTransactionId",
                principalTable: "PaymentTransactions",
                principalColumn: "PaymentTransactionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_AspNetUsers_UserId",
                table: "PaymentTransactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentTransactions_Donations_DonationId",
                table: "PaymentTransactions",
                column: "DonationId",
                principalTable: "Donations",
                principalColumn: "DonationId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactionDetails_PaymentTransactions_PaymentTransa~",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_AspNetUsers_UserId",
                table: "PaymentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentTransactions_Donations_DonationId",
                table: "PaymentTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentTransactions",
                table: "PaymentTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentTransactionDetails",
                table: "PaymentTransactionDetails");

            migrationBuilder.RenameTable(
                name: "PaymentTransactions",
                newName: "DonationTransactions");

            migrationBuilder.RenameTable(
                name: "PaymentTransactionDetails",
                newName: "DonationTransactionDetails");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentTransactions_UserId",
                table: "DonationTransactions",
                newName: "IX_DonationTransactions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentTransactions_DonationId",
                table: "DonationTransactions",
                newName: "IX_DonationTransactions_DonationId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentTransactionDetails_PaymentTransactionId",
                table: "DonationTransactionDetails",
                newName: "IX_DonationTransactionDetails_PaymentTransactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DonationTransactions",
                table: "DonationTransactions",
                column: "PaymentTransactionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DonationTransactionDetails",
                table: "DonationTransactionDetails",
                column: "PaymentTransactionDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_DonationTransactionDetails_DonationTransactions_PaymentTran~",
                table: "DonationTransactionDetails",
                column: "PaymentTransactionId",
                principalTable: "DonationTransactions",
                principalColumn: "PaymentTransactionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DonationTransactions_AspNetUsers_UserId",
                table: "DonationTransactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DonationTransactions_Donations_DonationId",
                table: "DonationTransactions",
                column: "DonationId",
                principalTable: "Donations",
                principalColumn: "DonationId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
