
using VitaTrack.Core.Common;

namespace VitaTrack.Core.Entities
{
    public class WeightTrack: BaseEntity<long>
    {
        public DateOnly DateRecordedOn { get; set; }
        public decimal Weight { get; set; }
    }
}
