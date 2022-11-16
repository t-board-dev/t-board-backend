using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace t_board.Entity
{
    public class Brand
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CompanyId { get; set; }

        public string Name { get; set; }

        public string Keywords { get; set; }

        public string Design { get; set; }

        public virtual Company Company { get; set; }
    }
}
