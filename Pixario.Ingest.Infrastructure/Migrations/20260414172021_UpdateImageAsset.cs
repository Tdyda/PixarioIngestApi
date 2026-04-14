using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pixario.Ingest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateImageAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "file_name",
                table: "image_assets");

            migrationBuilder.AlterColumn<string>(
                name: "storage_path",
                table: "image_assets",
                type: "varchar(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "original_file_name",
                table: "image_assets",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "stored_file_name",
                table: "image_assets",
                type: "char(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "original_file_name",
                table: "image_assets");

            migrationBuilder.DropColumn(
                name: "stored_file_name",
                table: "image_assets");

            migrationBuilder.AlterColumn<string>(
                name: "storage_path",
                table: "image_assets",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1024)",
                oldMaxLength: 1024)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "file_name",
                table: "image_assets",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
