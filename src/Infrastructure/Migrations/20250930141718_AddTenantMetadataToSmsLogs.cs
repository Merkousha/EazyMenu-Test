using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EazyMenu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantMetadataToSmsLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubscriptionPlan",
                table: "SmsDeliveryLogs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SmsDeliveryLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SmsDeliveryLogs_TenantId",
                table: "SmsDeliveryLogs",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SmsDeliveryLogs_TenantId",
                table: "SmsDeliveryLogs");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlan",
                table: "SmsDeliveryLogs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SmsDeliveryLogs");
        }
    }
}
