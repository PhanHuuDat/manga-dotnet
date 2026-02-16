using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manga.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttachmentScrambleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "manga_series",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<int>(
                name: "ScrambleGridSize",
                table: "attachments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScrambleSeed",
                table: "attachments",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "manga_series");

            migrationBuilder.DropColumn(
                name: "ScrambleGridSize",
                table: "attachments");

            migrationBuilder.DropColumn(
                name: "ScrambleSeed",
                table: "attachments");
        }
    }
}
