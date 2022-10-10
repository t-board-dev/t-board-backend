using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.User
{
    public class CheckInvitationRequest
    {
        [Required]
        [JsonProperty("inviteCode")]
        public string InviteCode { get; set; }
    }
}
