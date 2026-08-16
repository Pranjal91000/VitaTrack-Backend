namespace VitaTrack.Core.Models
{
    public class GetWeightByDate
    {
        public DateOnly RecordedOn { get; set; }
        public decimal Weight { get; set; }
    }
}
