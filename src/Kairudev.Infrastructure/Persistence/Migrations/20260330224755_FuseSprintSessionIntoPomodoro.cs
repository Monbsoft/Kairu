using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kairudev.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FuseSprintSessionIntoPomodoro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SprintSessions peut ne pas exister en prod si la migration AddSprintSessions
            // n'y a jamais été appliquée — on protège le DROP avec IF EXISTS.
            migrationBuilder.Sql("IF OBJECT_ID(N'[SprintSessions]', N'U') IS NOT NULL DROP TABLE [SprintSessions];");

            // JournalComment peut déjà exister si le schéma a divergé (même raison que ci-dessus).
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[PomodoroSessions]')
                    AND name = N'JournalComment'
                )
                ALTER TABLE [PomodoroSessions] ADD [JournalComment] nvarchar(500) NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Symétrique avec le Up() défensif : on ne supprime JournalComment que si elle existe.
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'[PomodoroSessions]')
                    AND name = N'JournalComment'
                )
                ALTER TABLE [PomodoroSessions] DROP COLUMN [JournalComment];
            ");

            // Ce Down() recrée SprintSessions de façon simplifiée.
            // SprintSession étant conceptuellement très proche de PomodoroSession,
            // la contrainte OwnsOne (SprintName) n'est pas restaurée.
            // Rollback volontairement partiel — SprintSession est définitivement supprimé.
            migrationBuilder.CreateTable(
                name: "SprintSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LinkedTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SprintSessions", x => x.Id);
                });
        }
    }
}
