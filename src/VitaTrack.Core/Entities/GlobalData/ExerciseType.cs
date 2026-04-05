using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VitaTrack.Core.Entities.GlobalData
{
    [Table("ExerciseType", Schema = "globaldata")]
    public class ExerciseType
    {
        [Key]
        public short ExerciseTypeNo { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string? ExerciseDescription { get; set; }
        public short statusNo { get; set; }
        public string? statusRemark { get; set; }
    }
}
