using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.Company
{
    public class UpdateCompanyRequest
    {
        [Required]
        [JsonProperty("id")]
        public int Id { get; set; }

        [Required]
        [JsonProperty("name")]
        public string Name { get; set; }

        [Required]
        [JsonProperty("type")]
        public int Type { get; set; }

        [Required]
        [JsonProperty("logoURL")]
        public string LogoURL { get; set; }
    }
}
