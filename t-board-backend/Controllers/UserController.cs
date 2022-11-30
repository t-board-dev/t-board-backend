using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using t_board.Entity;
using t_board.Services.Contracts;
using t_board_backend.Models.User;

namespace t_board_backend.Controllers
{
    [Route("user/")]
    public class UserController : Controller
    {
        private readonly IInviteService _inviteService;
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;

        private readonly UserManager<TBoardUser> _userManager;
        private readonly SignInManager<TBoardUser> _signInManager;

        private readonly TBoardDbContext _dbContext;

        public UserController(
            IInviteService inviteService,
            IJwtService jwtService,
            IUserService userService,
            UserManager<TBoardUser> userManager,
            SignInManager<TBoardUser> signInManager,
            TBoardDbContext dbContext)
        {
            _inviteService = inviteService;
            _jwtService = jwtService;
            _userService = userService;

            _userManager = userManager;
            _signInManager = signInManager;

            _dbContext = dbContext;
        }

        [HttpPost("signIn")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest signInRequest)
        {
            if (ModelState.IsValid is false) return BadRequest(signInRequest);

            var user = await _userManager.FindByEmailAsync(signInRequest.Email);
            if (user is null) return NotFound(signInRequest.Email);

            var canSignIn = await _signInManager.CanSignInAsync(user);
            var checkPassword = await _signInManager.CheckPasswordSignInAsync(user, signInRequest.Password, false);

            var result = await _signInManager.PasswordSignInAsync(user.UserName, signInRequest.Password, false, false);
            if (result.Succeeded)
            {
                var claims = await _userService.GetUserClaims(user);
                var token = _jwtService.GenerateToken(claims);

                HttpContext.Response.Cookies.Append("access_token", token, new CookieOptions { HttpOnly = true });

                return Ok(token);
            }

            if (result.IsLockedOut) return Forbid("Account is locked!");

            return BadRequest("Check credentials!");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("createUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest createUserRequest)
        {
            if (ModelState.IsValid is false) return BadRequest(createUserRequest);

            var user = new TBoardUser
            {
                FirstName = createUserRequest.FirstName,
                LastName = createUserRequest.LastName,
                UserName = createUserRequest.Email,
                Email = createUserRequest.Email,
                Title = createUserRequest.Title,
                PhoneNumber = createUserRequest.PhoneNumber
            };

            var created = await _userManager.CreateAsync(user);

            if (created.Succeeded is false) return UnprocessableEntity(created.Errors);

            var invitationSent = await _inviteService.SendInvitation(user.Email);
            if (invitationSent.Succeeded is false) return UnprocessableEntity(invitationSent.Message);

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("sendInvitation")]
        public async Task<IActionResult> SendInvitation([FromBody] SendInvitationRequest invitationRequest)
        {
            if (ModelState.IsValid is false) return BadRequest(invitationRequest);

            var invitationSent = await _inviteService.SendInvitation(invitationRequest.Email);

            if (invitationSent.Succeeded is false) return UnprocessableEntity(invitationSent.Message);

            return Ok();
        }

        [HttpPost("checkInvitation")]
        public async Task<IActionResult> CheckInvitation([FromBody] CheckInvitationRequest checkInvitationRequest)
        {
            if (ModelState.IsValid is false) return BadRequest(checkInvitationRequest);

            var invitation = await _dbContext.UserInvitations
                .Where(i => i.InviteCode == checkInvitationRequest.InviteCode)
                .FirstOrDefaultAsync();

            if (invitation == null) return NotFound(checkInvitationRequest.InviteCode);
            if (invitation.IsConfirmed) return Conflict("Already confirmed!");
            if (invitation.ExpireDate < DateTime.Now) Conflict("Invitation has expired!");

            return Ok();
        }

        [HttpPost("setPassword/{inviteCode}")]
        public async Task<IActionResult> SetPassword(string inviteCode, [FromBody] SetPasswordRequest setPasswordRequest)
        {
            if (ModelState.IsValid is false) return BadRequest(setPasswordRequest);

            if (string.Equals(setPasswordRequest.Password, setPasswordRequest.ConfirmPassword) is false) return BadRequest("Passwords does not match!");

            var invitation = await _dbContext.UserInvitations
                .Where(i => i.InviteCode == inviteCode)
                .FirstOrDefaultAsync();

            if (invitation == null) return NotFound(inviteCode);
            if (invitation.IsConfirmed) return Conflict("Invitation has already confirmed!");
            if (invitation.ExpireDate < DateTime.Now) return Conflict("Invitation has expired!");

            var user = await _userManager.FindByEmailAsync(invitation.UserEmail);
            if (user == null) return NotFound("User not found!");

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (hasPassword) return Conflict("User has already password!");

            var passwordAdded = await _userManager.AddPasswordAsync(user, setPasswordRequest.Password);
            if (passwordAdded.Succeeded is false) return UnprocessableEntity(passwordAdded.Errors);

            var emailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (emailConfirmed is false)
            {
                var emailConfirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmEmailResult = await _userManager.ConfirmEmailAsync(user, emailConfirmToken);
                if (confirmEmailResult.Succeeded is false) return UnprocessableEntity(confirmEmailResult.Errors);
            }

            var phoneConfirmed = await _userManager.IsPhoneNumberConfirmedAsync(user);
            if (phoneConfirmed is false)
            {
                var phoneConfirmToken = await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                var confirmPhoneResult = await _userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, phoneConfirmToken);
                if (confirmPhoneResult.Succeeded is false) return UnprocessableEntity(confirmPhoneResult.Errors);
            }

            invitation.IsConfirmed = true;
            invitation.ConfirmDate = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("signOut")]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}
