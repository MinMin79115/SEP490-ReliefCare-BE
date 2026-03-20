using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReliefManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPayOsDonationFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthenticationStatus",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "GatewayCustomerId",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "GatewayId",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "PayloadCreatedAt",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "AuthenticationStatus",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "CardBrand",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "CardExpiry",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "CardFundingMethod",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "CardHolderName",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "CardNumber",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "GatewayTransactionId",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "TransactionAmount",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "TransactionCurrency",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "TransactionDate",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "TransactionLastUpdatedDate",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "TransactionStatus",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "IsAnonymous",
                table: "Donations");

            migrationBuilder.RenameColumn(
                name: "PayloadUpdatedAt",
                table: "PaymentTransactions",
                newName: "TransactionDateTime");

            migrationBuilder.RenameColumn(
                name: "OrderStatus",
                table: "PaymentTransactions",
                newName: "VirtualAccountNumber");

            migrationBuilder.RenameColumn(
                name: "OrderInvoiceNumber",
                table: "PaymentTransactions",
                newName: "VirtualAccountName");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "PaymentTransactions",
                newName: "CounterAccountNumber");

            migrationBuilder.RenameColumn(
                name: "OrderDescription",
                table: "PaymentTransactions",
                newName: "CounterAccountName");

            migrationBuilder.RenameColumn(
                name: "OrderCurrency",
                table: "PaymentTransactions",
                newName: "CounterAccountBankName");

            migrationBuilder.RenameColumn(
                name: "OrderAmount",
                table: "PaymentTransactions",
                newName: "Amount");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "PaymentTransactions",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EventCode",
                table: "PaymentTransactions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventDescription",
                table: "PaymentTransactions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSignatureValid",
                table: "PaymentTransactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "OrderCode",
                table: "PaymentTransactions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "PaymentLinkId",
                table: "PaymentTransactions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "PaymentTransactions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RawPayload",
                table: "PaymentTransactions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "PaymentTransactions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Signature",
                table: "PaymentTransactions",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FieldName",
                table: "PaymentTransactionDetails",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FieldValue",
                table: "PaymentTransactionDetails",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DonorName",
                table: "Donations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckoutUrl",
                table: "Donations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Donations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "PayOsOrderCode",
                table: "Donations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayOsPaymentLinkId",
                table: "Donations",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_Provider_OrderCode_PaymentLinkId",
                table: "PaymentTransactions",
                columns: new[] { "Provider", "OrderCode", "PaymentLinkId" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_Provider_Reference",
                table: "PaymentTransactions",
                columns: new[] { "Provider", "Reference" });

            migrationBuilder.CreateIndex(
                name: "IX_Donations_PayOsOrderCode",
                table: "Donations",
                column: "PayOsOrderCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_Provider_OrderCode_PaymentLinkId",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_Provider_Reference",
                table: "PaymentTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Donations_PayOsOrderCode",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "EventCode",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "EventDescription",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "IsSignatureValid",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "OrderCode",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "PaymentLinkId",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "RawPayload",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "Signature",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "FieldName",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "FieldValue",
                table: "PaymentTransactionDetails");

            migrationBuilder.DropColumn(
                name: "CheckoutUrl",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "PayOsOrderCode",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "PayOsPaymentLinkId",
                table: "Donations");

            migrationBuilder.RenameColumn(
                name: "VirtualAccountNumber",
                table: "PaymentTransactions",
                newName: "OrderStatus");

            migrationBuilder.RenameColumn(
                name: "VirtualAccountName",
                table: "PaymentTransactions",
                newName: "OrderInvoiceNumber");

            migrationBuilder.RenameColumn(
                name: "TransactionDateTime",
                table: "PaymentTransactions",
                newName: "PayloadUpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CounterAccountNumber",
                table: "PaymentTransactions",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "CounterAccountName",
                table: "PaymentTransactions",
                newName: "OrderDescription");

            migrationBuilder.RenameColumn(
                name: "CounterAccountBankName",
                table: "PaymentTransactions",
                newName: "OrderCurrency");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "PaymentTransactions",
                newName: "OrderAmount");

            migrationBuilder.AddColumn<string>(
                name: "AuthenticationStatus",
                table: "PaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GatewayCustomerId",
                table: "PaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GatewayId",
                table: "PaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PayloadCreatedAt",
                table: "PaymentTransactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthenticationStatus",
                table: "PaymentTransactionDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardBrand",
                table: "PaymentTransactionDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardExpiry",
                table: "PaymentTransactionDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardFundingMethod",
                table: "PaymentTransactionDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardHolderName",
                table: "PaymentTransactionDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CardNumber",
                table: "PaymentTransactionDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GatewayTransactionId",
                table: "PaymentTransactionDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "PaymentTransactionDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransactionAmount",
                table: "PaymentTransactionDetails",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TransactionCurrency",
                table: "PaymentTransactionDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransactionDate",
                table: "PaymentTransactionDetails",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransactionLastUpdatedDate",
                table: "PaymentTransactionDetails",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionStatus",
                table: "PaymentTransactionDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionType",
                table: "PaymentTransactionDetails",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DonorName",
                table: "Donations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymous",
                table: "Donations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
