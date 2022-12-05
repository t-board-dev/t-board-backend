using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace t_board.Entity
{
    public class Company
    {
        public Company()
        {
            Brands = new HashSet<Brand>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Type { get; set; }

        public string LogoURL { get; set; }

        public virtual ICollection<Brand> Brands { get; set; }
    }
}
