using System.ComponentModel.DataAnnotations.Schema;

namespace t_board.Entity
{
    public class CompanyType
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }
    }
}
