using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using t_board.Entity;
using t_board_backend.Models.Role;

namespace t_board_backend.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("role/")]
    public class RoleController : Controller
    {
        private readonly UserManager<TBoardUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly TBoardDbContext _dbContext;
        public RoleController(
            UserManager<TBoardUser> userManager,
            RoleManager<IdentityRole> roleManager,
            TBoardDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;

            _dbContext = dbContext;
        }

        [HttpGet("getAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {
            return Ok(_roleManager.Roles?.Select(r => r.NormalizedName).ToArray() ?? Array.Empty<string>());
        }

        [HttpPost("getUserRoles")]
        public async Task<IActionResult> GetUserRoles([FromBody] GetUserRolesRequest getUserRolesRequest)
        {
            var user = await _userManager.FindByEmailAsync(getUserRolesRequest.Email);
            if (user == null) return NotFound(getUserRolesRequest.Email);

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(JsonSerializer.Serialize(roles));
        }
    }
}
