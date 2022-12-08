using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.User
{
    public class SetPasswordRequest
    {
        [Required]
        [JsonProperty("password")]
        public string Password { get; set; }
        
        [Required]
        [JsonProperty("confirmPassword")]
        public string ConfirmPassword { get; set; }

        [Required]
        [JsonProperty("inviteCode")]
        public string InviteCode { get; set; }
    }
}
