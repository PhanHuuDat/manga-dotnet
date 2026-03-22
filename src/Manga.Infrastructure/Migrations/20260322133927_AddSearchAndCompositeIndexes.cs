using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manga.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSearchAndCompositeIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

            migrationBuilder.DropIndex(
                name: "IX_comments_MangaSeriesId",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "IX_chapters_Slug",
                table: "chapters");

            migrationBuilder.CreateIndex(
                name: "IX_manga_series_Title",
                table: "manga_series",
                column: "Title")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_comments_MangaSeriesId_ParentId",
                table: "comments",
                columns: new[] { "MangaSeriesId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_chapters_MangaSeriesId_Slug",
                table: "chapters",
                columns: new[] { "MangaSeriesId", "Slug" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_manga_series_Title",
                table: "manga_series");

            migrationBuilder.DropIndex(
                name: "IX_comments_MangaSeriesId_ParentId",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "IX_chapters_MangaSeriesId_Slug",
                table: "chapters");

            migrationBuilder.CreateIndex(
                name: "IX_comments_MangaSeriesId",
                table: "comments",
                column: "MangaSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_chapters_Slug",
                table: "chapters",
                column: "Slug");

            migrationBuilder.Sql("DROP EXTENSION IF EXISTS pg_trgm;");
        }
    }
}
