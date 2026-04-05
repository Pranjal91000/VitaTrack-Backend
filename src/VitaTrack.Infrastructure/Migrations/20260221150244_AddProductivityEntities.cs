using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VitaTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductivityEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DistanceKm",
                table: "Sets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ParentId",
                table: "Activities",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DevelopmentTopics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "DsaProblems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: true),
                    Difficulty = table.Column<string>(type: "text", nullable: false),
                    ConfidenceLevel = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DsaProblems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DsaProblems_Users_UserId",
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
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    ActivityId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ParentId",
                table: "Activities",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentTopics_UserId",
                table: "DevelopmentTopics",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DsaProblems_UserId",
                table: "DsaProblems",
                column: "UserId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_Activities_ParentId",
                table: "Activities",
                column: "ParentId",
                principalTable: "Activities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_Activities_ParentId",
                table: "Activities");

            migrationBuilder.DropTable(
                name: "DevelopmentTopics");

            migrationBuilder.DropTable(
                name: "DsaProblems");

            migrationBuilder.DropTable(
                name: "TrackedTasks");

            migrationBuilder.DropIndex(
                name: "IX_Activities_ParentId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "DistanceKm",
                table: "Sets");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Activities");
        }
    }
}
