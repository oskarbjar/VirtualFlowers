using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models.Models
{
    public class FootballTips
    {

        [Key]
        public int Id { get; set; }
        [NotMapped]
        public string Content { get; set; }
    }
}
