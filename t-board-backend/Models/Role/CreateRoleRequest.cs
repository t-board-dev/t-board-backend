using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace t_board_backend.Models.Role
{
    public class CreateRoleRequest
    {
        [Required]
        [JsonProperty("roleName")]
        public string RoleName { get; set; }
    }
}
