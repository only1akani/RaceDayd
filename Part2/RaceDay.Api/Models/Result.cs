using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{//start of namespace

    //maps to the Results table in the database
    [Table("Results")]
    public class Result
    {//start of class

        [Key]
        [Column("result_id")]
        public int ResultId { get; set; }

        [Column("enrolment_id")]
        public int EnrolmentId { get; set; }

        //each result belongs to one enrolment
        [ForeignKey("EnrolmentId")]
        public Enrolment? Enrolment { get; set; }

        //time column in SQL maps to TimeSpan in C#
        [Column("finish_time", TypeName = "time")]
        public TimeSpan? FinishTime { get; set; }

        [Column("position")]
        public int? Position { get; set; }

        [Column("recorded_at")]
        public DateTime RecordedAt { get; set; } = DateTime.Now;

    }//end of class
}//end of namespace