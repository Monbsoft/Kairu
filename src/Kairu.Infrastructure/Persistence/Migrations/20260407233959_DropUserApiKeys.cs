using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kairu.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropUserApiKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserApiKeys");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KeyHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserApiKeys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserApiKeys_KeyHash",
                table: "UserApiKeys",
                column: "KeyHash",
                unique: true);
        }
    }
}
