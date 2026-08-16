using VitaTrack.Api.Workouts.DTOs;

namespace VitaTrack.Api.Abstractions
{
    public interface IWorkoutService
    {
        Task<DailyWorkoutsDto?> GetWorkoutsAsync(long userId, string dateString, CancellationToken cancellationToken = default);
        Task<WorkoutDto> CreateWorkoutAsync(long userId, CreateWorkoutRequest request, CancellationToken cancellationToken = default);
        Task<(WorkoutDto? Workout, string? Error)> AppendExercisesAsync(long id, long userId, AppendExercisesRequest request, CancellationToken cancellationToken = default);
        Task<(WorkoutDto? Workout, string? Error)> UpdateWorkoutAsync(long id, long userId, CreateWorkoutRequest request, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteWorkoutAsync(long id, long userId, CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<WorkoutHeatmapDayDto>? Heatmap, string? Error)> GetWorkoutHeatmapAsync(long userId, string from, string to, CancellationToken cancellationToken = default);
    }
}
