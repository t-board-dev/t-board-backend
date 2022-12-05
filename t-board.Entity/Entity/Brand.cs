using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace t_board.Entity
{
    public class Brand
    {
        public Brand()
        {
            Boards = new HashSet<Board>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CompanyId { get; set; }

        public string Name { get; set; }

        public string LogoURL { get; set; }

        public string Keywords { get; set; }

        public string Design { get; set; }

        public virtual Company Company { get; set; }

        public virtual ICollection<Board> Boards { get; set; }
    }
}
