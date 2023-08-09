using System;

namespace t_board_backend.Models.Board.Dto
{
    public class BoardItemDto
    {
        public BoardItemDto()
        {
            
        }

        public BoardItemDto(t_board.Entity.BoardItem boardItem)
        {
            Id = boardItem.Id;
            BoardId = boardItem.BoardId;
            Title = boardItem.Title;
            Type = boardItem.Type;
            GridData = boardItem.GridData;
            CustomGridData = boardItem.CustomGridData;
            Data = boardItem.Data;
            CreateDate = boardItem.CreateDate;
            UpdateDate = boardItem.UpdateDate;
            CreateUser = boardItem.CreateUser;
            UpdateUser = boardItem.UpdateUser;
        }

        public int Id { get; set; }

        public string BoardId { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public string GridData { get; set; }

        public string CustomGridData { get; set; }

        public string Data { get; set; }

        public DateTimeOffset? CreateDate { get; set; }

        public DateTimeOffset? UpdateDate { get; set; }

        public string CreateUser { get; set; }

        public string UpdateUser { get; set; }
    }
}
