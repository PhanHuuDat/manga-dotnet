using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manga.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddViewStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "view_stats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViewDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ViewCount = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    UniqueViewCount = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_view_stats", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_view_stats_TargetId",
                table: "view_stats",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_view_stats_TargetType_TargetId_ViewDate",
                table: "view_stats",
                columns: new[] { "TargetType", "TargetId", "ViewDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_view_stats_TargetType_ViewDate",
                table: "view_stats",
                columns: new[] { "TargetType", "ViewDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "view_stats");
        }
    }
}
