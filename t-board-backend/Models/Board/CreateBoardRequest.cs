using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.Board
{
    public class CreateBoardRequest
    {
        [Required]
        [JsonProperty("brandId")]
        public int BrandId { get; set; }

        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required]
        [JsonProperty("description")]
        public string Description { get; set; }

        [Required]
        [JsonProperty("status")]
        public short Status { get; set; }

        [Required]
        [JsonProperty("design")]
        public string Design { get; set; }
    }
}
