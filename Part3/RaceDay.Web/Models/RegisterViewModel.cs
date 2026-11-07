using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{//start of namespace

    //the data collected on the register page
    public class RegisterViewModel
    {//start of class

        [Required(ErrorMessage = "Please enter your full name.")]
        [StringLength(100, ErrorMessage = "Your name can be at most 100 characters.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your ID number.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Your ID number must be between 6 and 20 characters.")]
        [Display(Name = "ID number")]
        public string IdNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose a password.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Your password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "The passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        //Participant or Organiser
        [Required(ErrorMessage = "Please choose a role.")]
        [RegularExpression("^(Participant|Organiser)$", ErrorMessage = "Please choose Participant or Organiser.")]
        public string Role { get; set; } = "Participant";

    }//end of class
}//end of namespace