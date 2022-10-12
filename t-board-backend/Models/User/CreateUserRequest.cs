using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.User
{
    public class CreateUserRequest
    {
        [Required]
        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [Required]
        [JsonProperty("lastName")]
        public string LastName { get; set; }
        [Required]
        [JsonProperty("email")]
        public string Email { get; set; }
        [Required]
        [JsonProperty("title")]
        public string Title { get; set; }
        [Required]
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }
    }
}
