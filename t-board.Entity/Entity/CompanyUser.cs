using System.ComponentModel.DataAnnotations.Schema;

namespace t_board.Entity
{
    public class CompanyUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CompanyId { get; set; }

        public string UserId { get; set; }
    }
}
