using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VitaTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NutritionWorkoutScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "DevelopmentTopics");

            migrationBuilder.DropTable(
                name: "DsaProblems");

            migrationBuilder.DropTable(
                name: "TaskLogs");

            migrationBuilder.DropTable(
                name: "TrackedTasks");

            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Meals_UserId",
                table: "Meals");

            migrationBuilder.CreateTable(
                name: "MealSlots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealSlots_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.Sql("""
                INSERT INTO "MealSlots" ("UserId", "Name", "SortOrder", "CreatedAt", "IsDeleted") VALUES
                (NULL, 'Breakfast', 0, NOW() AT TIME ZONE 'utc', false),
                (NULL, 'Lunch', 1, NOW() AT TIME ZONE 'utc', false),
                (NULL, 'Dinner', 2, NOW() AT TIME ZONE 'utc', false),
                (NULL, 'Snack', 3, NOW() AT TIME ZONE 'utc', false);
                """);

            migrationBuilder.AddColumn<long>(
                name: "MealSlotId",
                table: "Meals",
                type: "bigint",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Meals" m SET "MealSlotId" = (SELECT "Id" FROM "MealSlots" s WHERE s."UserId" IS NULL AND s."Name" = 'Breakfast' LIMIT 1)
                WHERE lower(trim(m."Name")) IN ('breakfast');
                UPDATE "Meals" m SET "MealSlotId" = (SELECT "Id" FROM "MealSlots" s WHERE s."UserId" IS NULL AND s."Name" = 'Lunch' LIMIT 1)
                WHERE lower(trim(m."Name")) IN ('lunch');
                UPDATE "Meals" m SET "MealSlotId" = (SELECT "Id" FROM "MealSlots" s WHERE s."UserId" IS NULL AND s."Name" = 'Dinner' LIMIT 1)
                WHERE lower(trim(m."Name")) IN ('dinner');
                UPDATE "Meals" m SET "MealSlotId" = (SELECT "Id" FROM "MealSlots" s WHERE s."UserId" IS NULL AND s."Name" = 'Snack' LIMIT 1)
                WHERE lower(trim(m."Name")) IN ('snack');
                UPDATE "Meals" m SET "MealSlotId" = (SELECT "Id" FROM "MealSlots" s WHERE s."UserId" IS NULL AND s."Name" = 'Snack' LIMIT 1)
                WHERE m."MealSlotId" IS NULL;
                """);

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Meals");

            migrationBuilder.AlterColumn<long>(
                name: "MealSlotId",
                table: "Meals",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DemoMediaContentType",
                table: "Exercises",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DemoMediaFileName",
                table: "Exercises",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DemoMediaId",
                table: "Exercises",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Meals_MealSlotId",
                table: "Meals",
                column: "MealSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Meals_UserId_Date",
                table: "Meals",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_MealSlots_UserId",
                table: "MealSlots",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meals_MealSlots_MealSlotId",
                table: "Meals",
                column: "MealSlotId",
                principalTable: "MealSlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meals_MealSlots_MealSlotId",
                table: "Meals");

            migrationBuilder.DropTable(
                name: "MealSlots");

            migrationBuilder.DropIndex(
                name: "IX_Meals_MealSlotId",
                table: "Meals");

            migrationBuilder.DropIndex(
                name: "IX_Meals_UserId_Date",
                table: "Meals");

            migrationBuilder.DropColumn(
                name: "MealSlotId",
                table: "Meals");

            migrationBuilder.DropColumn(
                name: "DemoMediaContentType",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "DemoMediaFileName",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "DemoMediaId",
                table: "Exercises");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Meals",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Meals_UserId",
                table: "Meals",
                column: "UserId");

            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    Icon = table.Column<string>(type: "text", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Activities_Activities_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Activities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Activities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DevelopmentTopics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevelopmentTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevelopmentTopics_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActivityId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Tags = table.Column<string[]>(type: "text[]", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityLogs_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ActivityLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackedTasks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActivityId = table.Column<long>(type: "bigint", nullable: true),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    RecurrencePattern = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackedTasks_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "Activities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrackedTasks_TrackedTasks_ParentId",
                        column: x => x.ParentId,
                        principalTable: "TrackedTasks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TrackedTasks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DsaProblems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ConfidenceScore = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    HardnessLevel = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ProblemHeading = table.Column<string>(type: "text", nullable: false),
                    RefPageNumber = table.Column<string>(type: "text", nullable: true),
                    RevisitCount = table.Column<int>(type: "integer", nullable: false),
                    RevisitNeeded = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DsaProblems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DsaProblems_TrackedTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "TrackedTasks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DsaProblems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TaskId = table.Column<long>(type: "bigint", nullable: false),
                    Completed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "IX_Activities_ParentId",
                table: "Activities",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Activities_UserId",
                table: "Activities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_ActivityId",
                table: "ActivityLogs",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_UserId_Date",
                table: "ActivityLogs",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentTopics_UserId",
                table: "DevelopmentTopics",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DsaProblems_TaskId",
                table: "DsaProblems",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_DsaProblems_UserId",
                table: "DsaProblems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskLogs_TaskId",
                table: "TaskLogs",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackedTasks_ActivityId",
                table: "TrackedTasks",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackedTasks_ParentId",
                table: "TrackedTasks",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackedTasks_UserId",
                table: "TrackedTasks",
                column: "UserId");
        }
    }
}
