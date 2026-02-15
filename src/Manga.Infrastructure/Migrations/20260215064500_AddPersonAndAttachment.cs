using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Manga.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonAndAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_manga_series_Slug",
                table: "manga_series");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Artist",
                table: "manga_series");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "manga_series");

            migrationBuilder.DropColumn(
                name: "BannerUrl",
                table: "manga_series");

            migrationBuilder.DropColumn(
                name: "CoverUrl",
                table: "manga_series");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "manga_series");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "chapter_pages");

            migrationBuilder.AddColumn<Guid>(
                name: "AvatarId",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ArtistId",
                table: "manga_series",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "manga_series",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BannerId",
                table: "manga_series",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CoverId",
                table: "manga_series",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeriesNumber",
                table: "manga_series",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "chapter_pages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "persons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PersonNumber = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Biography = table.Column<string>(type: "text", nullable: true),
                    PhotoId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_persons_attachments_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_AvatarId",
                table: "users",
                column: "AvatarId");

            migrationBuilder.CreateIndex(
                name: "IX_manga_series_ArtistId",
                table: "manga_series",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_manga_series_AuthorId",
                table: "manga_series",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_manga_series_BannerId",
                table: "manga_series",
                column: "BannerId");

            migrationBuilder.CreateIndex(
                name: "IX_manga_series_CoverId",
                table: "manga_series",
                column: "CoverId");

            migrationBuilder.CreateIndex(
                name: "IX_manga_series_SeriesNumber",
                table: "manga_series",
                column: "SeriesNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chapter_pages_ImageId",
                table: "chapter_pages",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_attachments_Type",
                table: "attachments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_persons_Name",
                table: "persons",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_persons_PersonNumber",
                table: "persons",
                column: "PersonNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_persons_PhotoId",
                table: "persons",
                column: "PhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_chapter_pages_attachments_ImageId",
                table: "chapter_pages",
                column: "ImageId",
                principalTable: "attachments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_manga_series_attachments_BannerId",
                table: "manga_series",
                column: "BannerId",
                principalTable: "attachments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_manga_series_attachments_CoverId",
                table: "manga_series",
                column: "CoverId",
                principalTable: "attachments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_manga_series_persons_ArtistId",
                table: "manga_series",
                column: "ArtistId",
                principalTable: "persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_manga_series_persons_AuthorId",
                table: "manga_series",
                column: "AuthorId",
                principalTable: "persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_users_attachments_AvatarId",
                table: "users",
                column: "AvatarId",
                principalTable: "attachments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_chapter_pages_attachments_ImageId",
                table: "chapter_pages");

            migrationBuilder.DropForeignKey(
                name: "FK_manga_series_attachments_BannerId",
                table: "manga_series");

            migrationBuilder.DropForeignKey(
                name: "FK_manga_series_attachments_CoverId",
                table: "manga_series");

            migrationBuilder.DropForeignKey(
                name: "FK_manga_series_persons_ArtistId",
                table: "manga_series");

            migrationBuilder.DropForeignKey(
                name: "FK_manga_series_persons_AuthorId",
                table: "manga_series");

            migrationBuilder.DropForeignKey(
                name: "FK_users_attachments_AvatarId",
                table: "users");

            migrationBuilder.DropTable(
                name: "persons");

            migrationBuilder.DropTable(
                name: "attachments");

            migrationBuilder.DropIndex(
                name: "IX_users_AvatarId",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_manga_series_ArtistId",
                table: "manga_series");

            migrationBuilder.DropIndex(
                name: "IX_manga_series_AuthorId",
                table: "manga_series");

            migrationBuilder.DropIndex(
                name: "IX_manga_series_BannerId",
                table: "manga_series");

            migrationBuilder.DropIndex(
                name: "IX_manga_series_CoverId",
                table: "manga_series");

            migrationBuilder.DropIndex(
                name: "IX_manga_series_SeriesNumber",
                table: "manga_series");

            migrationBuilder.DropIndex(
                name: "IX_chapter_pages_ImageId",
                table: "chapter_pages");

            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ArtistId",
                table: "manga_series");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "manga_series");

            migrationBuilder.DropColumn(
                name: "BannerId",
                table: "manga_series");

            migrationBuilder.DropColumn(
                name: "CoverId",
                table: "manga_series");

            migrationBuilder.DropColumn(
                name: "SeriesNumber",
                table: "manga_series");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "chapter_pages");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Artist",
                table: "manga_series",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "manga_series",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BannerUrl",
                table: "manga_series",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverUrl",
                table: "manga_series",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "manga_series",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "chapter_pages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_manga_series_Slug",
                table: "manga_series",
                column: "Slug",
                unique: true);
        }
    }
}
