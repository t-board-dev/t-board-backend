using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.Brand
{
    public class UpdateBrandUserRequest
    {
        [Required]
        [JsonProperty("companyId")]
        public int CompanyId { get; set; }
        [Required]
        [JsonProperty("brandId")]
        public int[] BrandIds { get; set; }
        [Required]
        [JsonProperty("userId")]
        public string UserId { get; set; }
    }
}
