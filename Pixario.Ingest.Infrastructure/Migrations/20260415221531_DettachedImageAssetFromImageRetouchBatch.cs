using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pixario.Ingest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DettachedImageAssetFromImageRetouchBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_image_assets_retouch_batch_batch_id",
                table: "image_assets");

            migrationBuilder.DropIndex(
                name: "IX_image_assets_batch_id",
                table: "image_assets");

            migrationBuilder.DropColumn(
                name: "batch_id",
                table: "image_assets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "batch_id",
                table: "image_assets",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_image_assets_batch_id",
                table: "image_assets",
                column: "batch_id");

            migrationBuilder.AddForeignKey(
                name: "FK_image_assets_retouch_batch_batch_id",
                table: "image_assets",
                column: "batch_id",
                principalTable: "retouch_batch",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
