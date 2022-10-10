using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.Role
{
    public class GetUserRolesRequest
    {
        [Required]
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
