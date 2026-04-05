using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VitaTrack.Core.Entities;

namespace VitaTrack.Infrastructure.Persistence;

public static class VitaTrackDbContextInitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initialiser = scope.ServiceProvider.GetRequiredService<VitaTrackDbContextInitialiser>();
        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class VitaTrackDbContextInitialiser
{
    private readonly ILogger<VitaTrackDbContextInitialiser> _logger;
    private readonly VitaTrackDbContext _context;

    public VitaTrackDbContextInitialiser(ILogger<VitaTrackDbContextInitialiser> logger, VitaTrackDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (_context.Database.IsNpgsql())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default Foods
        if (!_context.Foods.Any())
        {
            _context.Foods.AddRange(new List<Food>
            {
                new Food { Name = "Apple", ServingSize = 100, Unit = "g", Calories = 52, CarbsG = 14, FatG = 0.2m, ProteinG = 0.3m, FiberG = 2.4m, SugarG = 10, SodiumMg = 1 },
                new Food { Name = "Banana", ServingSize = 100, Unit = "g", Calories = 89, CarbsG = 23, FatG = 0.3m, ProteinG = 1.1m, FiberG = 2.6m, SugarG = 12, SodiumMg = 1 },
                new Food { Name = "Chicken Breast", ServingSize = 100, Unit = "g", Calories = 165, CarbsG = 0, FatG = 3.6m, ProteinG = 31, FiberG = 0, SugarG = 0, SodiumMg = 74 },
                new Food { Name = "Rice (White, Cooked)", ServingSize = 100, Unit = "g", Calories = 130, CarbsG = 28, FatG = 0.3m, ProteinG = 2.7m, FiberG = 0.4m, SugarG = 0.1m, SodiumMg = 1 },
                new Food { Name = "Broccoli", ServingSize = 100, Unit = "g", Calories = 34, CarbsG = 7, FatG = 0.4m, ProteinG = 2.8m, FiberG = 2.6m, SugarG = 1.7m, SodiumMg = 33 },
                new Food { Name = "Egg (Large)", ServingSize = 50, Unit = "g", Calories = 78, CarbsG = 0.6m, FatG = 5, ProteinG = 6, FiberG = 0, SugarG = 0.6m, SodiumMg = 62 },
                new Food { Name = "Oats (Rolled)", ServingSize = 100, Unit = "g", Calories = 389, CarbsG = 66, FatG = 7, ProteinG = 17, FiberG = 11, SugarG = 0, SodiumMg = 2 },
                new Food { Name = "Almonds", ServingSize = 100, Unit = "g", Calories = 579, CarbsG = 22, FatG = 50, ProteinG = 21, FiberG = 12, SugarG = 4, SodiumMg = 1 },
                new Food { Name = "Salmon", ServingSize = 100, Unit = "g", Calories = 208, CarbsG = 0, FatG = 13, ProteinG = 20, FiberG = 0, SugarG = 0, SodiumMg = 59 },
                new Food { Name = "Spinach", ServingSize = 100, Unit = "g", Calories = 23, CarbsG = 3.6m, FatG = 0.4m, ProteinG = 2.9m, FiberG = 2.2m, SugarG = 0.4m, SodiumMg = 79 },
            });
            await _context.SaveChangesAsync();
        }

        if (!_context.MealSlots.Any(ms => ms.UserId == null))
        {
            _context.MealSlots.AddRange(new List<MealSlot>
            {
                new MealSlot { UserId = null, Name = "Breakfast", SortOrder = 0 },
                new MealSlot { UserId = null, Name = "Lunch", SortOrder = 1 },
                new MealSlot { UserId = null, Name = "Dinner", SortOrder = 2 },
                new MealSlot { UserId = null, Name = "Snack", SortOrder = 3 },
            });
            await _context.SaveChangesAsync();
        }

        // Default Exercises
        //if (!_context.Exercises.Any())
        //{
        //    _context.Exercises.AddRange(new List<Exercise>
        //     {
        //         new Exercise { Name = "Bench Press", Type = ExerciseType.Strength, MuscleGroups = new[] { "Chest", "Triceps", "Shoulders" }, IsDefault = true },
        //         new Exercise { Name = "Squat", Type = ExerciseType.Strength, MuscleGroups = new[] { "Legs", "Glutes", "Core" }, IsDefault = true },
        //         new Exercise { Name = "Deadlift", Type = ExerciseType.Strength, MuscleGroups = new[] { "Back", "Legs", "Glutes" }, IsDefault = true },
        //         new Exercise { Name = "Overhead Press", Type = ExerciseType.Strength, MuscleGroups = new[] { "Shoulders", "Triceps" }, IsDefault = true },
        //         new Exercise { Name = "Pull Up", Type = ExerciseType.Strength, MuscleGroups = new[] { "Back", "Biceps" }, IsDefault = true },
        //         new Exercise { Name = "Dumbbell Row", Type = ExerciseType.Strength, MuscleGroups = new[] { "Back", "Biceps" }, IsDefault = true },
        //         new Exercise { Name = "Lunges", Type = ExerciseType.Strength, MuscleGroups = new[] { "Legs", "Glutes" }, IsDefault = true },
        //         new Exercise { Name = "Plank", Type = ExerciseType.Strength, MuscleGroups = new[] { "Core" }, IsDefault = true },
        //         new Exercise { Name = "Bicep Curl", Type = ExerciseType.Strength, MuscleGroups = new[] { "Biceps" }, IsDefault = true },
        //         new Exercise { Name = "Tricep Extension", Type = ExerciseType.Strength, MuscleGroups = new[] { "Triceps" }, IsDefault = true },
        //     });
        //    await _context.SaveChangesAsync();
        //}
    }
}
