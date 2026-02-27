using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kairudev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalEntrySequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "JournalEntries",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "JournalEntries");
        }
    }
}
