using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{//start of namespace

    //maps to the Roles table in the database
    [Table("Roles")]
    public class Role
    {//start of class

        [Key]
        [Column("role_id")]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("role_name")]
        public string RoleName { get; set; } = string.Empty;

        //one role has many users
        public List<User> Users { get; set; } = new List<User>();

    }//end of class
}//end of namespace