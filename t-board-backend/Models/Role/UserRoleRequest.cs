using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.Role
{
    public class UserRoleRequest
    {
        [Required]
        [JsonProperty("roleName")]
        public string RoleName { get; set; }
        [Required]
        [JsonProperty("userEmail")]
        public string UserEmail { get; set; }
    }
}
