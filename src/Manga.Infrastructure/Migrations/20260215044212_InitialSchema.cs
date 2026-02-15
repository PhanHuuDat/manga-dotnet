using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Manga.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "genres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "manga_series",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Slug = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Synopsis = table.Column<string>(type: "text", nullable: true),
                    CoverUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BannerUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Author = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Artist = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Badge = table.Column<int>(type: "integer", nullable: true),
                    PublishedYear = table.Column<int>(type: "integer", nullable: true),
                    Rating = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false, defaultValue: 0m),
                    RatingCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Views = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    TotalChapters = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LatestChapterNumber = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manga_series", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Level = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Role = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "alternative_titles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MangaSeriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alternative_titles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_alternative_titles_manga_series_MangaSeriesId",
                        column: x => x.MangaSeriesId,
                        principalTable: "manga_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chapters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MangaSeriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChapterNumber = table.Column<decimal>(type: "numeric(6,1)", precision: 6, scale: 1, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Slug = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Pages = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Views = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chapters_manga_series_MangaSeriesId",
                        column: x => x.MangaSeriesId,
                        principalTable: "manga_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manga_genres",
                columns: table => new
                {
                    MangaSeriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    GenreId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manga_genres", x => new { x.MangaSeriesId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_manga_genres_genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_manga_genres_manga_series_MangaSeriesId",
                        column: x => x.MangaSeriesId,
                        principalTable: "manga_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bookmarks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MangaSeriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookmarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bookmarks_manga_series_MangaSeriesId",
                        column: x => x.MangaSeriesId,
                        principalTable: "manga_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bookmarks_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chapter_pages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PageNumber = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chapter_pages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chapter_pages_chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "chapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Likes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Dislikes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MangaSeriesId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChapterId = table.Column<Guid>(type: "uuid", nullable: true),
                    PageNumber = table.Column<int>(type: "integer", nullable: true),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReplyCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comments_chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "chapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_comments_comments_ParentId",
                        column: x => x.ParentId,
                        principalTable: "comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comments_manga_series_MangaSeriesId",
                        column: x => x.MangaSeriesId,
                        principalTable: "manga_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_comments_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reading_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MangaSeriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastPageNumber = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    LastReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reading_histories_chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "chapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reading_histories_manga_series_MangaSeriesId",
                        column: x => x.MangaSeriesId,
                        principalTable: "manga_series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reading_histories_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comment_reactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReactionType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comment_reactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comment_reactions_comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_comment_reactions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "genres",
                columns: new[] { "Id", "Description", "Name", "Slug" },
                values: new object[,]
                {
                    { new Guid("4e226e46-6956-5af8-ad13-e4d13b323a71"), null, "Supernatural", "supernatural" },
                    { new Guid("65f5c210-d15b-5ad0-a78b-dc43eb7d534f"), null, "Adventure", "adventure" },
                    { new Guid("66034ab9-e670-5b65-92c1-a38a218532bf"), null, "Slice of Life", "slice-of-life" },
                    { new Guid("694ab696-833b-51e2-88e1-30d2781fcf7d"), null, "Horror", "horror" },
                    { new Guid("763e0632-e274-5340-9caa-9a18da2ef68f"), null, "Drama", "drama" },
                    { new Guid("7abfa697-2dd9-5b45-a677-9e2d9bd35034"), null, "Romance", "romance" },
                    { new Guid("9fe6f39b-b520-5dd8-be5f-de03165f4e95"), null, "Fantasy", "fantasy" },
                    { new Guid("a02f8514-8e15-559b-8001-c5cf6783e18c"), null, "Mystery", "mystery" },
                    { new Guid("b18be532-0987-55c1-8c44-689d1ad423dd"), null, "Sci-Fi", "sci-fi" },
                    { new Guid("bd5b5b91-e53d-54a5-992a-71d15437f80f"), null, "Action", "action" },
                    { new Guid("f6c4c585-9625-5e7c-9ac9-0482bb720808"), null, "Sports", "sports" },
                    { new Guid("fbdbaec9-255d-5b0b-96db-dc534ca5bb4e"), null, "Comedy", "comedy" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_alternative_titles_MangaSeriesId",
                table: "alternative_titles",
                column: "MangaSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_bookmarks_MangaSeriesId",
                table: "bookmarks",
                column: "MangaSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_bookmarks_UserId_MangaSeriesId",
                table: "bookmarks",
                columns: new[] { "UserId", "MangaSeriesId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chapter_pages_ChapterId_PageNumber",
                table: "chapter_pages",
                columns: new[] { "ChapterId", "PageNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chapters_MangaSeriesId_ChapterNumber",
                table: "chapters",
                columns: new[] { "MangaSeriesId", "ChapterNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chapters_PublishedAt",
                table: "chapters",
                column: "PublishedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_chapters_Slug",
                table: "chapters",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_comment_reactions_CommentId",
                table: "comment_reactions",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_comment_reactions_UserId_CommentId",
                table: "comment_reactions",
                columns: new[] { "UserId", "CommentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_comments_ChapterId",
                table: "comments",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_comments_CreatedAt",
                table: "comments",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_comments_MangaSeriesId",
                table: "comments",
                column: "MangaSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_comments_ParentId",
                table: "comments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_comments_UserId",
                table: "comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_genres_Name",
                table: "genres",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_genres_Slug",
                table: "genres",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_manga_genres_GenreId",
                table: "manga_genres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_manga_series_CreatedAt",
                table: "manga_series",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_manga_series_Slug",
                table: "manga_series",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_manga_series_Status",
                table: "manga_series",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_manga_series_Views",
                table: "manga_series",
                column: "Views",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_reading_histories_ChapterId",
                table: "reading_histories",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_reading_histories_MangaSeriesId",
                table: "reading_histories",
                column: "MangaSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_reading_histories_UserId_MangaSeriesId",
                table: "reading_histories",
                columns: new[] { "UserId", "MangaSeriesId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                table: "users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alternative_titles");

            migrationBuilder.DropTable(
                name: "bookmarks");

            migrationBuilder.DropTable(
                name: "chapter_pages");

            migrationBuilder.DropTable(
                name: "comment_reactions");

            migrationBuilder.DropTable(
                name: "manga_genres");

            migrationBuilder.DropTable(
                name: "reading_histories");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "genres");

            migrationBuilder.DropTable(
                name: "chapters");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "manga_series");
        }
    }
}
