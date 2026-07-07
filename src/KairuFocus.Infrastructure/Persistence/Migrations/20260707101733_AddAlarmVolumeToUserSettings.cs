using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KairuFocus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlarmVolumeToUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlarmVolume",
                table: "UserSettings",
                type: "int",
                nullable: false,
                defaultValue: 80);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlarmVolume",
                table: "UserSettings");
        }
    }
}
