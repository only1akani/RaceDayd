using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{//start of namespace

    //maps to the categories table in the database
    [Table("categories")]
    public class Category
    {//start of class

        [Key]
        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("event_id")]
        public int EventId { get; set; }

        //each category belongs to one event
        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("category_name")]
        public string CategoryName { get; set; } = string.Empty;

        [Column("distance", TypeName = "decimal(5,2)")]
        public decimal Distance { get; set; }

        [Column("price", TypeName = "decimal(8,2)")]
        public decimal Price { get; set; } = 0;

        [Column("max_participants")]
        public int MaxParticipants { get; set; } = 100;

    }//end of class
}//end of namespace