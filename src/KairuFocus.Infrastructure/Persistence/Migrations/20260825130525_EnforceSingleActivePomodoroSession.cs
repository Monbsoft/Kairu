using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KairuFocus.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleActivePomodoroSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Status and SessionType must leave nvarchar(max): SQL Server forbids LOB
            //    columns in the predicate of a filtered index, and Status is used by the
            //    filter of the unique index created below.
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PomodoroSessions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SessionType",
                table: "PomodoroSessions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // 2. Defensive clean-up BEFORE creating the unique index.
            //    The bug this migration fixes could already have produced several Active
            //    sessions for the same user; the index creation would then fail on an
            //    existing database. Keep the most recently started one and close the others
            //    as Interrupted (they were never completed by the user anyway).
            migrationBuilder.Sql("""
                ;WITH DuplicateActiveSessions AS (
                    SELECT
                        [Id],
                        ROW_NUMBER() OVER (
                            PARTITION BY [OwnerId]
                            ORDER BY [StartedAt] DESC, [Id] DESC) AS [RowNumber]
                    FROM [PomodoroSessions]
                    WHERE [Status] = N'Active' AND [OwnerId] IS NOT NULL
                )
                UPDATE [p]
                SET [p].[Status] = N'Interrupted',
                    [p].[EndedAt] = SYSUTCDATETIME()
                FROM [PomodoroSessions] AS [p]
                INNER JOIN [DuplicateActiveSessions] AS [d] ON [d].[Id] = [p].[Id]
                WHERE [d].[RowNumber] > 1;
                """);

            // 3. At most one Active session per user, enforced by the database.
            //    "[OwnerId] IS NOT NULL" is required: OwnerId is optional and SQL Server
            //    treats NULLs as equal in a unique index, so legacy NULL-owner rows would
            //    make the index creation fail. The N'' prefix matches the nvarchar Status
            //    column: a non-Unicode literal would force a CONVERT_IMPLICIT and prevent the
            //    optimiser from using the index for the queries EF emits.
            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSessions_OwnerId_ActiveUnique",
                table: "PomodoroSessions",
                column: "OwnerId",
                unique: true,
                filter: "[Status] = N'Active' AND [OwnerId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Schema-reversible only — the duplicate clean-up (step 2) is NOT reversible:
            // sessions closed as Interrupted stay closed.
            // Defensive drop: the index may already be absent (partially applied migration).
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE [name] = 'IX_PomodoroSessions_OwnerId_ActiveUnique'
                      AND [object_id] = OBJECT_ID(N'[dbo].[PomodoroSessions]'))
                    DROP INDEX [IX_PomodoroSessions_OwnerId_ActiveUnique] ON [dbo].[PomodoroSessions];
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PomodoroSessions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "SessionType",
                table: "PomodoroSessions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }
    }
}
