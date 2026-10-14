using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{//start of namespace

    //the data needed to create or edit an event
    public class EventDto
    {//start of class

        [Required]
        [MaxLength(100)]
        public string EventName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? EventDescription { get; set; }

        //nullable so a missing date is caught as a 400
        [Required]
        public DateTime? EventDate { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

    }//end of class
}//end of namespace