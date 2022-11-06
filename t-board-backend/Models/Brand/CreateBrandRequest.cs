using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.Brand
{
    public class CreateBrandRequest
    {
        [Required]
        [JsonProperty("brandName")]
        public string Name { get; set; }
        [Required]
        [JsonProperty("keywords")]
        public string Keywords { get; set; }
        [Required]
        [JsonProperty("logoUrl")]
        public string LogoUrl { get; set; }
    }
}
