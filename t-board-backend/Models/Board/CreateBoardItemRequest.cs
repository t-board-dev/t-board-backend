using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.Board
{
    public class CreateBoardItemRequest
    {
        [Required]
        [JsonProperty("boardId")]
        public int BoardId { get; set; }

        [Required]
        [JsonProperty("title")]
        public string Title { get; set; }

        [Required]
        [JsonProperty("type")]
        public int Type { get; set; }

        [Required]
        [JsonProperty("gridData")]
        public string GridData { get; set; }

        [Required]
        [JsonProperty("customGridData")]
        public string CustomGridData { get; set; }

        [Required]
        [JsonProperty("Data")]
        public string Data { get; set; }
    }
}
