namespace VitaTrack.Api.Models.WeightTracker
{
    public class WeightTrackerViewModel
    {
        public DateOnly RecordedOn { get; set; }
        public decimal Weight { get; set; }
    }
}
