using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VitaTrack.Core.Common;
using VitaTrack.Core.Entities;
using VitaTrack.Core.Entities.GlobalData;

namespace VitaTrack.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Food> Foods => Set<Food>();
    public DbSet<MealSlot> MealSlots => Set<MealSlot>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<MealFood> MealFoods => Set<MealFood>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutExercise> WorkoutExercises => Set<WorkoutExercise>();
    public DbSet<Set> Sets => Set<Set>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Status> Statuses => Set<Status>();
    public DbSet<ExerciseType> ExeciseTypes => Set<ExerciseType>();
    public DbSet<WeightTracker> WeightTracks => Set<WeightTracker>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity<Guid>>())
        {
            switch (entry.State)
            {
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Entity<Food>()
            .HasIndex(x => x.Name);

        builder.Entity<Meal>()
            .HasIndex(x => new { x.UserId, x.Date });

        builder.Entity<WeightTracker>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Food>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<MealSlot>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Meal>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<MealFood>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Exercise>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Workout>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<WorkoutExercise>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Set>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<RefreshToken>().HasQueryFilter(e => !e.IsDeleted);
    }
}
