namespace VitaTrack.Api.Models.WeightTrack
{

    public class WeightTrackBaseInputModel
    {
        public DateOnly RecordedOn;
        public decimal Weight;
    }

    public class WeightTrackSaveInputModel: WeightTrackBaseInputModel
    { 
    }

    public class WeightTrackUpdateInputModel: WeightTrackBaseInputModel
    {
        public long Id;
    }
}
