using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{//start of namespace

    //maps to the Enrolments table in the database
    [Table("Enrolments")]
    public class Enrolment
    {//start of class

        [Key]
        [Column("enrolment_id")]
        public int EnrolmentId { get; set; }

        //the participant who entered the event
        [Column("participant_id")]
        public int ParticipantId { get; set; }

        [ForeignKey("ParticipantId")]
        public User? Participant { get; set; }

        //the category the participant chose
        [Column("category_id")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [Column("enrolment_date")]
        public DateTime EnrolmentDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        [Column("status")]
        public string Status { get; set; } = "Pending";

        //one enrolment has at most one result
        public Result? Result { get; set; }

    }//end of class
}//end of namespace