using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EazyMenu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BranchQrCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchQrCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BranchTables",
                columns: table => new
                {
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    IsOutdoor = table.Column<bool>(type: "bit", nullable: false),
                    IsOutOfService = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchTables", x => x.TableId);
                });

            migrationBuilder.CreateTable(
                name: "BranchWorkingHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    Start = table.Column<TimeSpan>(type: "time", nullable: false),
                    End = table.Column<TimeSpan>(type: "time", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchWorkingHours", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantBranches",
                columns: table => new
                {
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Street = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantBranches", x => x.BranchId);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BrandDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BrandLogoUrl = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    BrandPrimaryColor = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    HeadquartersCity = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    HeadquartersStreet = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    HeadquartersPostalCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    IsSuspended = table.Column<bool>(type: "bit", nullable: false),
                    SuspendedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActiveSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantSubscriptions",
                columns: table => new
                {
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Plan = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PriceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceCurrency = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    StartDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsTrial = table.Column<bool>(type: "bit", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DiscountCode = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantSubscriptions", x => x.SubscriptionId);
                    table.ForeignKey(
                        name: "FK_TenantSubscriptions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchQrCodes_BranchId",
                table: "BranchQrCodes",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchTables_BranchId",
                table: "BranchTables",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchWorkingHours_BranchId",
                table: "BranchWorkingHours",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantBranches_TenantId",
                table: "TenantBranches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_ActiveSubscriptionId",
                table: "Tenants",
                column: "ActiveSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSubscriptions_TenantId",
                table: "TenantSubscriptions",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_BranchQrCodes_TenantBranches_BranchId",
                table: "BranchQrCodes",
                column: "BranchId",
                principalTable: "TenantBranches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BranchTables_TenantBranches_BranchId",
                table: "BranchTables",
                column: "BranchId",
                principalTable: "TenantBranches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BranchWorkingHours_TenantBranches_BranchId",
                table: "BranchWorkingHours",
                column: "BranchId",
                principalTable: "TenantBranches",
                principalColumn: "BranchId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantBranches_Tenants_TenantId",
                table: "TenantBranches",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_TenantSubscriptions_ActiveSubscriptionId",
                table: "Tenants",
                column: "ActiveSubscriptionId",
                principalTable: "TenantSubscriptions",
                principalColumn: "SubscriptionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TenantSubscriptions_Tenants_TenantId",
                table: "TenantSubscriptions");

            migrationBuilder.DropTable(
                name: "BranchQrCodes");

            migrationBuilder.DropTable(
                name: "BranchTables");

            migrationBuilder.DropTable(
                name: "BranchWorkingHours");

            migrationBuilder.DropTable(
                name: "TenantBranches");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "TenantSubscriptions");
        }
    }
}
