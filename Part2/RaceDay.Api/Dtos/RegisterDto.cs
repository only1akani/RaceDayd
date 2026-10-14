using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{//start of namespace

    //the data needed to register a new user
    public class RegisterDto
    {//start of class

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string IdNumber { get; set; } = string.Empty;

        //must be Organiser or Participant
        [Required]
        public string Role { get; set; } = string.Empty;

    }//end of class
}//end of namespace