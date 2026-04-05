using VitaTrack.Core.Common;

namespace VitaTrack.Core.Entities;

public class User : BaseEntity<long>, IAggregateRoot
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int? Age { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? Bmr { get; set; }
    public string Role { get; set; } = "User";

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Meal> Meals { get; set; } = new List<Meal>();
    public ICollection<Workout> Workouts { get; set; } = new List<Workout>();
    public ICollection<MealSlot> MealSlots { get; set; } = new List<MealSlot>();
}
