using System.ComponentModel.DataAnnotations.Schema;

namespace t_board.Entity
{
    public class BrandUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int BrandId { get; set; }

        public string UserId { get; set; }
    }
}
