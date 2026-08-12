namespace VitaTrack.Api.Models.WeightTrack
{

    public class WeightTrackBaseInputModel
    {
        public DateOnly RecordedOn { get; set; }
        public decimal Weight { get; set; }
    }

    public class WeightTrackSaveInputModel: WeightTrackBaseInputModel
    { 
    }

    public class WeightTrackUpdateInputModel: WeightTrackBaseInputModel
    {
        public long Id { get; set; }
    }
}
