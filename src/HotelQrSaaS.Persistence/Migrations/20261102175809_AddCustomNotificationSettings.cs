using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelQrSaaS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomNotificationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomSmtpHost",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomSmtpPassword",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomSmtpPort",
                table: "Tenants",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomSmtpUser",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelegramBotToken",
                table: "Tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelegramBotUsername",
                table: "Tenants",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomSmtpHost",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CustomSmtpPassword",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CustomSmtpPort",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "CustomSmtpUser",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "TelegramBotToken",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "TelegramBotUsername",
                table: "Tenants");
        }
    }
}
