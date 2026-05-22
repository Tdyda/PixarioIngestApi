using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pixario.Ingest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGalleryIdToBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "gallery_id",
                table: "retouch_batch",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "gallery_id",
                table: "retouch_batch");
        }
    }
}
