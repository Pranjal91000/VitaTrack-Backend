using VitaTrack.Core.Common;

namespace VitaTrack.Core.Entities;

public class Meal : BaseEntity<long>, IAggregateRoot
{
    public long UserId { get; set; }
    public long MealSlotId { get; set; }
    public DateOnly Date { get; set; }
    public string? Notes { get; set; }

    public User User { get; set; } = null!;
    public MealSlot MealSlot { get; set; } = null!;
    public ICollection<MealFood> MealFoods { get; set; } = new List<MealFood>();
}
