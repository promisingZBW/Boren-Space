using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyToAdminOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreateTime",
                value: new DateTime(2025, 9, 8, 18, 51, 33, 672, DateTimeKind.Local).AddTicks(4139));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreateTime",
                value: new DateTime(2025, 9, 8, 18, 51, 33, 672, DateTimeKind.Local).AddTicks(4183));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreateTime",
                value: new DateTime(2025, 8, 18, 21, 40, 42, 379, DateTimeKind.Local).AddTicks(5041));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreateTime",
                value: new DateTime(2025, 8, 18, 21, 40, 42, 379, DateTimeKind.Local).AddTicks(5085));
        }
    }
}
