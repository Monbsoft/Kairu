using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kairu.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMcpTokensForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_McpTokens_Users_UserId",
                table: "McpTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_McpTokens_Users_UserId",
                table: "McpTokens");
        }
    }
}
