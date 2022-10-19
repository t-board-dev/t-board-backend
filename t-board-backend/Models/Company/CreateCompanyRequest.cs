using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.Company
{
    public class CreateCompanyRequest
    {
        [Required]
        [JsonProperty("companyName")]
        public string CompanyName { get; set; }
        [Required]
        [JsonProperty("companyType")]
        public int CompanyType { get; set; }
        [Required]
        [JsonProperty("companyUrl")]
        public string CompanyUrl { get; set; }

        [Required]
        [JsonProperty("ownerFirstName")]
        public string OwnerFirstName { get; set; }
        [Required]
        [JsonProperty("ownerLastName")]
        public string OwnerLastName { get; set; }
        [Required]
        [JsonProperty("ownerEmail")]
        public string OwnerEmail { get; set; }
        [Required]
        [JsonProperty("ownerTitle")]
        public string OwnerTitle { get; set; }
        [Required]
        [JsonProperty("ownerPhoneNumber")]
        public string OwnerPhoneNumber { get; set; }
    }
}
