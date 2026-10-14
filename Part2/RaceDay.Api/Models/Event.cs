using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{//start of namespace

    //maps to the Events table in the database
    [Table("Events")]
    public class Event
    {//start of class

        [Key]
        [Column("event_id")]
        public int EventId { get; set; }

        //the organiser who owns this event
        [Column("organiser_id")]
        public int OrganiserId { get; set; }

        [ForeignKey("OrganiserId")]
        public User? Organiser { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("event_name")]
        public string EventName { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("event_description")]
        public string? EventDescription { get; set; }

        [Column("event_date")]
        public DateTime EventDate { get; set; }

        [MaxLength(150)]
        [Column("location")]
        public string? Location { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //one event has many categories
        public List<Category> Categories { get; set; } = new List<Category>();

    }//end of class
}//end of namespace
