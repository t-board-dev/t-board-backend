using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.Brand
{
    public class UpdateBrandRequest
    {
        [Required]
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required]
        [JsonProperty("logoURL")]
        public string LogoURL { get; set; }

        [Required]
        [JsonProperty("keywords")]
        public string Keywords { get; set; }

        [Required]
        [JsonProperty("design")]
        public string Design { get; set; }
    }
}
