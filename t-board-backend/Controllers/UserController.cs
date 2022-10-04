using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using t_board.Entity;
using t_board.Entity.Entity;
using t_board_backend.Models.User;

namespace t_board_backend.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;

        private readonly UserManager<TBoardUser> _userManager;
        private readonly SignInManager<TBoardUser> _signInManager;

        private readonly TBoardDbContext _dbContext;

        public UserController(
            IConfiguration configuration,
            UserManager<TBoardUser> userManager,
            SignInManager<TBoardUser> signInManager,
            TBoardDbContext dbContext)
        {
            _configuration = configuration;

            _userManager = userManager;
            _signInManager = signInManager;

            _dbContext = dbContext;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return new JsonResult("User could not found!");

            var canSignIn = await _signInManager.CanSignInAsync(user);
            var checkPassword = await _signInManager.CheckPasswordSignInAsync(user, password, false);

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, false, false);
            if (result.Succeeded)
            {
                var claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("UserName", user.UserName.ToString()),
                        new Claim("Email", user.Email),
                        new Claim("FirstName", user.FirstName),
                        new Claim("LastName", user.LastName),
                    };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(60),
                    signingCredentials: signIn);

                return new JsonResult(new JwtSecurityTokenHandler().WriteToken(token));
            }

            if (result.IsLockedOut) return new JsonResult("User account has locked!");

            return new JsonResult("Invalid login attempt!");
        }

        // TODO
        // Admin control
        // Only admin can create user
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser(UserDto user)
        {
            var created = await _userManager.CreateAsync(new TBoardUser
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            });

            if (created.Succeeded is false) return new JsonResult(created.Errors.Select(e => new string($"[{e.Code} - {e.Description}]")));

            var invitationSent = await SendInvitationCore(user.Email);
            if (invitationSent.Succeeded is false) return new JsonResult(invitationSent.Message);

            return new JsonResult(created.Succeeded);
        }

        // TODO
        // Admin control
        // Only admin can send invitation
        [HttpPost("SendInvitation")]
        public async Task<IActionResult> SendInvitation(string userEmail)
        {
            var invitationSent = await SendInvitationCore(userEmail);

            if (invitationSent.Succeeded is false) return new JsonResult(invitationSent.Message);

            return new JsonResult(invitationSent.Succeeded);
        }

        private async Task<(bool Succeeded, string Message)> SendInvitationCore(string userEmail)
        {
            var invitation = await _dbContext.UserInvitations.Where(i => i.UserEmail == userEmail).FirstOrDefaultAsync();

            if (invitation == null)
            {
                invitation = new UserInvitation
                {
                    UserEmail = userEmail,
                    InviteCode = Guid.NewGuid().ToString(),
                    InviteDate = DateTime.Now,
                    ExpireDate = DateTime.Today.AddDays(2),
                };

                _dbContext.Add(invitation);
            }
            else
            {
                if (invitation.IsConfirmed) return (false, "Invitation has already confirmed!");

                invitation.InviteDate = DateTime.Now;
                invitation.ExpireDate = DateTime.Today.AddDays(2);
            }

            await _dbContext.SaveChangesAsync();

            // TODO:
            // Send invitation

            return (true, "Invitation sent!");
        }

        [AllowAnonymous]
        [HttpGet("Invitation")]
        public async Task<IActionResult> Invitation([FromQuery(Name = "inviteCode")] string inviteCode)
        {
            var invitation = await _dbContext.UserInvitations
                .Where(i => i.InviteCode == inviteCode)
                .FirstOrDefaultAsync();

            if (invitation == null) return new JsonResult("Invitation has not found!");
            if (invitation.IsConfirmed) return new JsonResult("Invitation has already confirmed!");
            if (invitation.ExpireDate < DateTime.Now) return new JsonResult("Invitation has expired!");

            return new JsonResult(true);
        }

        [AllowAnonymous]
        [HttpPost("ConfirmUser")]
        public async Task<IActionResult> ConfirmUser([FromQuery(Name = "inviteCode")] string inviteCode, string password)
        {
            var invitation = await _dbContext.UserInvitations
                .Where(i => i.InviteCode == inviteCode)
                .FirstOrDefaultAsync();

            if (invitation == null) return new JsonResult("Invitation has not found!");
            if (invitation.IsConfirmed) return new JsonResult("Invitation has already confirmed!");
            if (invitation.ExpireDate < DateTime.Now) return new JsonResult("Invitation has expired!");

            var user = await _userManager.FindByEmailAsync(invitation.UserEmail);
            if (user == null) return new JsonResult("User not found!");
            if (user.EmailConfirmed) return new JsonResult("User has already confirmed!");

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (hasPassword) return new JsonResult("User has already password!");
            var passwordAdded = await _userManager.AddPasswordAsync(user, password);
            if (passwordAdded.Succeeded is false) return new JsonResult(passwordAdded.Errors.Select(e => $"[{e.Code} - {e.Description}]"));

            var emailConfirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var emailConfirmed = await _userManager.ConfirmEmailAsync(user, emailConfirmToken);
            if (emailConfirmed.Succeeded is false) return new JsonResult(emailConfirmed.Errors.Select(e => $"[{e.Code} - {e.Description}]"));

            var phoneConfirmToken = await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
            var phoneConfirmed = await _userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, phoneConfirmToken);
            if (phoneConfirmed.Succeeded is false) return new JsonResult(phoneConfirmed.Errors.Select(e => $"[{e.Code} - {e.Description}]"));

            invitation.IsConfirmed = true;
            invitation.ConfirmDate = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            return new JsonResult(true);
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return new JsonResult(true);
        }
    }
}
