using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{//start of namespace

    //the data needed to capture a result
    public class ResultDto
    {//start of class

        //format is hh:mm:ss, for example 00:48:32
        [Required]
        public string FinishTime { get; set; } = string.Empty;

        //nullable so a missing position is caught as a 400
        [Required]
        [Range(1, 100000)]
        public int? Position { get; set; }

    }//end of class
}//end of namespace