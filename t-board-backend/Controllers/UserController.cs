using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using System.Threading.Tasks;

namespace t_board_backend.Controllers
{
    public class UserController : Controller
    {
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login()
        {
            return new JsonResult(true);
        }

        [AllowAnonymous]
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser()
        {
            return new JsonResult(true);
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            return new JsonResult(true);
        }
    }
}
