using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elzahy.Migrations
{
    /// <inheritdoc />
    public partial class ConvertToFileSystemStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoData",
                table: "ProjectVideos");

            migrationBuilder.DropColumn(
                name: "ImageData",
                table: "ProjectImages");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "ProjectVideos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "ProjectVideos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "ProjectImages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "ProjectImages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVideos_FilePath",
                table: "ProjectVideos",
                column: "FilePath");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectImages_FilePath",
                table: "ProjectImages",
                column: "FilePath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectVideos_FilePath",
                table: "ProjectVideos");

            migrationBuilder.DropIndex(
                name: "IX_ProjectImages_FilePath",
                table: "ProjectImages");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "ProjectVideos");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "ProjectVideos");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "ProjectImages");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "ProjectImages");

            migrationBuilder.AddColumn<byte[]>(
                name: "VideoData",
                table: "ProjectVideos",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageData",
                table: "ProjectImages",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
