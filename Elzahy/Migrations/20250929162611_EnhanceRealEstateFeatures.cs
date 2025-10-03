using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elzahy.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceRealEstateFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Direction",
                table: "ProjectTranslations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Projects",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Projects",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriceCurrency",
                table: "Projects",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceEnd",
                table: "Projects",
                type: "decimal(20,2)",
                precision: 20,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceStart",
                table: "Projects",
                type: "decimal(20,2)",
                precision: 20,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProjectArea",
                table: "Projects",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropertyType",
                table: "Projects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalUnits",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectVideos_SortOrder",
                table: "ProjectVideos",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTranslations_Language",
                table: "ProjectTranslations",
                column: "Language");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsFeatured",
                table: "Projects",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsPublished",
                table: "Projects",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Location",
                table: "Projects",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_PropertyType",
                table: "Projects",
                column: "PropertyType");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SortOrder",
                table: "Projects",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status",
                table: "Projects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectImages_SortOrder",
                table: "ProjectImages",
                column: "SortOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectVideos_SortOrder",
                table: "ProjectVideos");

            migrationBuilder.DropIndex(
                name: "IX_ProjectTranslations_Language",
                table: "ProjectTranslations");

            migrationBuilder.DropIndex(
                name: "IX_Projects_IsFeatured",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_IsPublished",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Location",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_PropertyType",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_SortOrder",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_Status",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_ProjectImages_SortOrder",
                table: "ProjectImages");

            migrationBuilder.DropColumn(
                name: "Direction",
                table: "ProjectTranslations");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PriceCurrency",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PriceEnd",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PriceStart",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectArea",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "PropertyType",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "TotalUnits",
                table: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
