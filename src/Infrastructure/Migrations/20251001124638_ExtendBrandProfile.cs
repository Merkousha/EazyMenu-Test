using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EazyMenu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendBrandProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrandAboutText",
                table: "Tenants",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandBannerImageUrl",
                table: "Tenants",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "BrandIsPublished",
                table: "Tenants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "BrandOpeningHours",
                table: "Tenants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandSecondaryColor",
                table: "Tenants",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandTemplateName",
                table: "Tenants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrandAboutText",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BrandBannerImageUrl",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BrandIsPublished",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BrandOpeningHours",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BrandSecondaryColor",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "BrandTemplateName",
                table: "Tenants");
        }
    }
}
