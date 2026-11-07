using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{//start of namespace

    //the data needed to change the status of an enrolment
    public class EnrolmentStatusDto
    {//start of class

        //Pending or Confirmed
        [Required]
        public string Status { get; set; } = string.Empty;

    }//end of class
}//end of namespace