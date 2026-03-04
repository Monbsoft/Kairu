using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kairudev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMultiUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PomodoroSettings",
                table: "PomodoroSettings");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PomodoroSettings");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "UserSettings",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Tasks",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PomodoroSettings",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "PomodoroSessions",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "JournalEntries",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PomodoroSettings",
                table: "PomodoroSettings",
                column: "UserId");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    GitHubId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Login = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_GitHubId",
                table: "Users",
                column: "GitHubId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PomodoroSettings",
                table: "PomodoroSettings");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PomodoroSettings");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "PomodoroSessions");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "JournalEntries");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "PomodoroSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PomodoroSettings",
                table: "PomodoroSettings",
                column: "Id");
        }
    }
}
