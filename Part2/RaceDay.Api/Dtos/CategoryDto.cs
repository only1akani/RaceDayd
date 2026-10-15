using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{//start of namespace

    //the data needed to create or edit a category
    public class CategoryDto
    {//start of class

        [Required]
        [MaxLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        //nullable so a missing distance is caught as a 400
        [Required]
        [Range(0.01, 1000)]
        public decimal? Distance { get; set; }

        //optional, defaults to 0 (free)
        [Range(0, 100000)]
        public decimal? Price { get; set; }

        //optional, defaults to 100
        [Range(1, 100000)]
        public int? MaxParticipants { get; set; }

    }//end of class
}//end of namespace