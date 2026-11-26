using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WalletLedger.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedSystemAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "BalanceMinorUnits", "Currency", "Kind", "OwnerUserId" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0001-000000000001"), 0L, "AZN", "SystemClearing", null },
                    { new Guid("00000000-0000-0000-0001-000000000002"), 0L, "AZN", "SystemHold", null },
                    { new Guid("00000000-0000-0000-0001-000000000003"), 0L, "USD", "SystemClearing", null },
                    { new Guid("00000000-0000-0000-0001-000000000004"), 0L, "USD", "SystemHold", null },
                    { new Guid("00000000-0000-0000-0001-000000000005"), 0L, "EUR", "SystemClearing", null },
                    { new Guid("00000000-0000-0000-0001-000000000006"), 0L, "EUR", "SystemHold", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000001"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000002"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000003"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000004"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000005"));

            migrationBuilder.DeleteData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0001-000000000006"));
        }
    }
}
