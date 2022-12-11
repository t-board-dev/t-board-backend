using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.Brand
{
    public class CreateBrandRequest
    {
        [Required]
        [JsonProperty("companyId")]
        public int CompanyId { get; set; }
        
        [Required]
        [JsonProperty("brandName")]
        public string Name { get; set; }

        [JsonProperty("logoUrl")]
        public string LogoURL { get; set; }
        
        [Required]
        [JsonProperty("keywords")]
        public string Keywords { get; set; }

        [JsonProperty("design")]
        public string Design { get; set; }
    }
}
