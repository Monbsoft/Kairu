using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kairudev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJiraTicketKeyToTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JiraApiToken",
                table: "UserSettings",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JiraBaseUrl",
                table: "UserSettings",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JiraEmail",
                table: "UserSettings",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JiraTicketKey",
                table: "Tasks",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JiraApiToken",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "JiraBaseUrl",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "JiraEmail",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "JiraTicketKey",
                table: "Tasks");
        }
    }
}
