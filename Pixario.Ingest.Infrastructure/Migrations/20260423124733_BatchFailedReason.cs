using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pixario.Ingest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BatchFailedReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "batch_failed_reason",
                table: "retouch_batch",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "batch_failed_reason",
                table: "retouch_batch");
        }
    }
}
