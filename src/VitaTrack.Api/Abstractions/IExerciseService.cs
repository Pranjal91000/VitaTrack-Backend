using Microsoft.AspNetCore.Http;
using VitaTrack.Api.Common.Models;
using VitaTrack.Api.Workouts.DTOs;

namespace VitaTrack.Api.Abstractions
{
    public interface IExerciseService
    {
        Task<ApiResponse<List<ExerciseDto>>> GetExercisesAsync(string search, int page, int limit, CancellationToken cancellationToken = default);
        Task<ExerciseDto> CreateExerciseAsync(long userId, CreateExerciseRequest request, CancellationToken cancellationToken = default);
        Task<(ExerciseDto? Exercise, string? Error)> UpdateExerciseAsync(long id, long userId, UpdateExerciseRequest request, CancellationToken cancellationToken = default);
        Task<(bool Success, string? Error)> DeleteExerciseAsync(long id, long userId, string contentRootPath, CancellationToken cancellationToken = default);
        Task<(ExerciseDto? Exercise, string? Error)> UploadDemoMediaAsync(long id, long userId, IFormFile file, string contentRootPath, CancellationToken cancellationToken = default);
        Task<(string? PhysicalPath, string? ContentType, string? Error)> GetDemoMediaAsync(long id, long userId, string contentRootPath, CancellationToken cancellationToken = default);
        Task<(ExerciseDto? Exercise, string? Error)> DeleteDemoMediaAsync(long id, long userId, string contentRootPath, CancellationToken cancellationToken = default);
    }
}
