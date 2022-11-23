﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace t_board.Entity
{
    public class Board
    {
        public Board()
        {
            BoardItems = new HashSet<BoardItem>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int BrandId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public short Status { get; set; }

        public string Design { get; set; }

        public virtual Brand Brand { get; set; }

        public virtual ICollection<BoardItem> BoardItems { get; set; }
    }
}