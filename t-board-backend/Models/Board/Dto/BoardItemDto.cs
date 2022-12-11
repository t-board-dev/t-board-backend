namespace t_board_backend.Models.Board.Dto
{
    public class BoardItemDto
    {
        public int Id { get; set; }

        public int BoardId { get; set; }

        public string Title { get; set; }

        public int Type { get; set; }

        public string GridData { get; set; }

        public string CustomGridData { get; set; }

        public string Data { get; set; }
    }
}
