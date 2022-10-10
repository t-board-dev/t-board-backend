using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.User
{
    public class SendInvitationRequest
    {
        [Required]
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
