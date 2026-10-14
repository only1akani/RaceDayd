using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{//start of namespace

    //the data a user can change on their own profile
    public class UpdateProfileDto
    {//start of class

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

    }//end of class
}//end of namespace
