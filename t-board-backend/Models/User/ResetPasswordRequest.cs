using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.User
{
    public class ResetPasswordRequest
    {
        [Required]
        [JsonProperty("password")]
        public string Password { get; set; }

        [Required]
        [JsonProperty("confirmPassword")]
        public string ConfirmPassword { get; set; }

        [Required]
        [JsonProperty("resetToken")]
        public string ResetToken { get; set; }

        [Required]
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
