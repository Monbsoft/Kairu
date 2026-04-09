using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kairu.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMcpTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "McpTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_McpTokens_TokenHash",
                table: "McpTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_McpTokens_UserId",
                table: "McpTokens",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Defensive drop: no-op if the table was already removed (partial rollback scenario).
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[McpTokens]', N'U') IS NOT NULL
                    DROP TABLE [McpTokens];
                """);
        }
    }
}
