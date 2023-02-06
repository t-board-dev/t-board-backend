using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.Brand
{
    public class CreateBrandUserRequest
    {
        [Required]
        [JsonProperty("companyId")]
        public int CompanyId { get; set; }
        [Required]
        [JsonProperty("brandIds")]
        public int[] BrandIds { get; set; }
        [Required]
        [JsonProperty("userFirstName")]
        public string UserFirstName { get; set; }
        [Required]
        [JsonProperty("userLastName")]
        public string UserLastName { get; set; }
        [Required]
        [JsonProperty("userEmail")]
        public string UserEmail { get; set; }
        [Required]
        [JsonProperty("userTitle")]
        public string UserTitle { get; set; }
        [Required]
        [JsonProperty("userPhoneNumber")]
        public string UserPhoneNumber { get; set; }
    }
}
