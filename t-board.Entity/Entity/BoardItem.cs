using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace t_board.Entity
{
    public class BoardItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string BoardId { get; set; }

        public string Title { get; set; }
        
        public int Type { get; set; }

        public string GridData { get; set; }

        public string CustomGridData { get; set; }

        public string Data { get; set; }

        public DateTimeOffset CreateDate { get; set; }

        public DateTimeOffset? UpdateDate { get; set; }
        
        public string CreateUser { get; set; }

        public string UpdateUser { get; set; }

        public virtual Board Board { get; set; }
    }
}
