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

        [HttpGet("getRoles")]
        public async Task<IActionResult> GetRoles()
        {
            return Ok(_roleManager.Roles?.Select(r => r.NormalizedName).ToArray() ?? Array.Empty<string>());
        }

        [HttpPost("getUserRoles")]
        public async Task<IActionResult> GetUserRoles([FromBody] GetUserRolesRequest getUserRolesRequest)
        {
            if (ModelState.IsValid is false) return BadRequest(getUserRolesRequest);

            var user = await _userManager.FindByEmailAsync(getUserRolesRequest.Email);
            if (user == null) return NotFound(getUserRolesRequest.Email);

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(JsonSerializer.Serialize(roles));
        }

        [HttpPost("createRole")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest createRoleRequest)
        {
            if (ModelState.IsValid is false) return BadRequest(createRoleRequest);

            var roleCreated = await _roleManager.CreateAsync(new IdentityRole(createRoleRequest.RoleName));

            if (roleCreated.Succeeded is false) return UnprocessableEntity(roleCreated.Errors);

            return Ok();
        }

        [HttpPost("addRoleToUser")]
        public async Task<IActionResult> AddRoleToUser([FromBody] UserRoleRequest userRoleRequest)
        {
            if (ModelState.IsValid is false) return BadRequest(userRoleRequest);

            var userEmail = userRoleRequest.UserEmail;

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user is null) return NotFound(user);

            var roleName = userRoleRequest.RoleName;

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null) return NotFound(role);

            // AddToRoleAsync already checks IsInRoleAsync 
            var roleAdded = await _userManager.AddToRoleAsync(user, roleName);
            if (roleAdded.Succeeded is false) return Conflict(roleAdded.Errors);

            return Ok();
        }

        [HttpPost("removeUserRole")]
        public async Task<IActionResult> RemoveUserRole([FromBody] UserRoleRequest userRoleRequest)
        {
            if (ModelState.IsValid is false) return BadRequest(userRoleRequest);

            var userEmail = userRoleRequest.UserEmail;

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user is null) return NotFound(user);

            var roleName = userRoleRequest.RoleName;

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null) return NotFound(role);

            // RemoveFromRoleAsync already checks IsInRoleAsync 
            var roleRemoved = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (roleRemoved.Succeeded is false) return Conflict(roleRemoved.Errors);

            return Ok();
        }
    }
}
