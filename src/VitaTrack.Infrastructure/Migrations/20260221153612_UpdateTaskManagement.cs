using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VitaTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaskManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "TrackedTasks");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "TrackedTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Deadline",
                table: "TrackedTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "TrackedTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RecurrencePattern",
                table: "TrackedTasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TrackedTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TaskLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Completed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskLogs_TrackedTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "TrackedTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskLogs_TaskId",
                table: "TaskLogs",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskLogs");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "TrackedTasks");

            migrationBuilder.DropColumn(
                name: "Deadline",
                table: "TrackedTasks");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "TrackedTasks");

            migrationBuilder.DropColumn(
                name: "RecurrencePattern",
                table: "TrackedTasks");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TrackedTasks");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "TrackedTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
