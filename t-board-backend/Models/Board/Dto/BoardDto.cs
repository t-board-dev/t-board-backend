﻿namespace t_board_backend.Models.Board.Dto
{
    public class BoardDto
    {
        public int Id { get; set; }

        public int BrandId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public short Status { get; set; }

        public string Design { get; set; }

        public string CreateUser { get; set; }

        public string UpdateUser { get; set; }
    }
}
