using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UploadedItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileSizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    FileSHA256Hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FileType = table.Column<int>(type: "integer", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UploadTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UploaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BackupUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RemoteUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UploadedItems_FileSize_Hash",
                table: "UploadedItems",
                columns: new[] { "FileSizeInBytes", "FileSHA256Hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UploadedItems_IsDeleted",
                table: "UploadedItems",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedItems_UploaderId",
                table: "UploadedItems",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedItems_UploadTime",
                table: "UploadedItems",
                column: "UploadTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UploadedItems");
        }
    }
}
