using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{//start of namespace

    //the My Results page
    public class MyResultsViewModel
    {//start of class

        public List<MyResult> Results { get; set; } = new List<MyResult>();

        //finished events the participant entered that have no result yet
        public List<MyEnrolment> AwaitingResults { get; set; } = new List<MyEnrolment>();

    }//end of class

    //the profile page
    public class ProfileViewModel
    {//start of class

        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100, ErrorMessage = "Your name can be at most 100 characters.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        //shown on the page but not edited in the form
        public string? ProfilePictureUrl { get; set; }
        public string? Role { get; set; }

    }//end of class
}//end of namespace