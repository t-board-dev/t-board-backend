using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace t_board.Entity
{
    public class Company
    {
        public Company()
        {
            Brand = new HashSet<Brand>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Type { get; set; }

        public string Url { get; set; }

        public virtual ICollection<Brand> Brand { get; set; }
    }
}
