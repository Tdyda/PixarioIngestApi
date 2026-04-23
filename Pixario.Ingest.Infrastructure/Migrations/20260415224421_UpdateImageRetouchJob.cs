using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pixario.Ingest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateImageRetouchJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_retouch_jobs_image_assets_ImageId",
                table: "retouch_jobs");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "retouch_jobs",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "retouch_jobs",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ImageId",
                table: "retouch_jobs",
                newName: "image_id");

            migrationBuilder.RenameIndex(
                name: "IX_retouch_jobs_ImageId",
                table: "retouch_jobs",
                newName: "IX_retouch_jobs_image_id");

            migrationBuilder.AddForeignKey(
                name: "FK_retouch_jobs_image_assets_image_id",
                table: "retouch_jobs",
                column: "image_id",
                principalTable: "image_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_retouch_jobs_image_assets_image_id",
                table: "retouch_jobs");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "retouch_jobs",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "retouch_jobs",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "image_id",
                table: "retouch_jobs",
                newName: "ImageId");

            migrationBuilder.RenameIndex(
                name: "IX_retouch_jobs_image_id",
                table: "retouch_jobs",
                newName: "IX_retouch_jobs_ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_retouch_jobs_image_assets_ImageId",
                table: "retouch_jobs",
                column: "ImageId",
                principalTable: "image_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
