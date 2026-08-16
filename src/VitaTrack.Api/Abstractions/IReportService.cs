using VitaTrack.Api.Reports.DTOs;
using VitaTrack.Api.Workouts.DTOs;

namespace VitaTrack.Api.Abstractions
{
    public interface IReportService
    {
        Task<(NutritionReportDto? Report, string? Error)> GetNutritionReportAsync(long userId, string from, string to, CancellationToken cancellationToken = default);
        Task<(WorkoutReportDto? Report, string? Error)> GetWorkoutReportAsync(long userId, string from, string to, CancellationToken cancellationToken = default);
        Task<(ExerciseMonthlyReportDto? Report, string? Error)> GetExerciseMonthlyReportAsync(long userId, long exerciseId, string month, CancellationToken cancellationToken = default);
    }
}
