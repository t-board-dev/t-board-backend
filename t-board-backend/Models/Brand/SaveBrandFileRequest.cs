using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.Brand
{
    public class SaveBrandFileRequest
    {
        [Required]
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [Required]
        [JsonProperty("brandId")]
        public int BrandId { get; set; }
        
        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [Required]
        [JsonProperty("url")]
        public string URL { get; set; }
        
        [Required]
        [JsonProperty("status")]
        public short Status { get; set; }
    }
}
