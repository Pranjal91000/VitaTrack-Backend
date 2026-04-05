using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VitaTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTemplate",
                table: "Workouts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RecurrencePattern",
                table: "Workouts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ElevationGainM",
                table: "Sets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaceMinPerKm",
                table: "Sets",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MeasurementType",
                table: "Exercises",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTemplate",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "RecurrencePattern",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "ElevationGainM",
                table: "Sets");

            migrationBuilder.DropColumn(
                name: "PaceMinPerKm",
                table: "Sets");

            migrationBuilder.DropColumn(
                name: "MeasurementType",
                table: "Exercises");
        }
    }
}
