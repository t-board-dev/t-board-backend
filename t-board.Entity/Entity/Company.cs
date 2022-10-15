using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace t_board.Entity.Entity
{
    public class Company
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        [Required]
        public int Type { get; set; }

        [Required]
        [MaxLength(512)]
        public string Url { get; set; }
    }
}
