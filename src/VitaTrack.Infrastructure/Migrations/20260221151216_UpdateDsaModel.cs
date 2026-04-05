using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitaTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDsaModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "DsaProblems");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "DsaProblems",
                newName: "ProblemHeading");

            migrationBuilder.RenameColumn(
                name: "ConfidenceLevel",
                table: "DsaProblems",
                newName: "RevisitCount");

            migrationBuilder.AddColumn<int>(
                name: "ConfidenceScore",
                table: "DsaProblems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HardnessLevel",
                table: "DsaProblems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RefPageNumber",
                table: "DsaProblems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RevisitNeeded",
                table: "DsaProblems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "TaskId",
                table: "DsaProblems",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DsaProblems_TaskId",
                table: "DsaProblems",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_DsaProblems_TrackedTasks_TaskId",
                table: "DsaProblems",
                column: "TaskId",
                principalTable: "TrackedTasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DsaProblems_TrackedTasks_TaskId",
                table: "DsaProblems");

            migrationBuilder.DropIndex(
                name: "IX_DsaProblems_TaskId",
                table: "DsaProblems");

            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "DsaProblems");

            migrationBuilder.DropColumn(
                name: "HardnessLevel",
                table: "DsaProblems");

            migrationBuilder.DropColumn(
                name: "RefPageNumber",
                table: "DsaProblems");

            migrationBuilder.DropColumn(
                name: "RevisitNeeded",
                table: "DsaProblems");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "DsaProblems");

            migrationBuilder.RenameColumn(
                name: "RevisitCount",
                table: "DsaProblems",
                newName: "ConfidenceLevel");

            migrationBuilder.RenameColumn(
                name: "ProblemHeading",
                table: "DsaProblems",
                newName: "Title");

            migrationBuilder.AddColumn<string>(
                name: "Difficulty",
                table: "DsaProblems",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
