using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.User
{
    public class ChangePasswordRequest
    {
        [Required]
        [JsonProperty("currentPassword")]
        public string CurrentPassword { get; set; }

        [Required]
        [JsonProperty("password")]
        public string Password { get; set; }

        [Required]
        [JsonProperty("confirmPassword")]
        public string ConfirmPassword { get; set; }
    }
}
