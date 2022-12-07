﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using t_board.Entity;
using t_board.Services.Contracts;
using t_board_backend.Extensions;
using t_board_backend.Models.User;
using t_board_backend.Models.User.Dto;

namespace t_board_backend.Controllers
{
    [Route("user/")]
    public class UserController : Controller
    {
        private readonly IMailService _mailService;
        private readonly IInviteService _inviteService;
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        private readonly IConfiguration Configuration;

        private readonly UserManager<TBoardUser> _userManager;
        private readonly SignInManager<TBoardUser> _signInManager;

        private readonly TBoardDbContext _dbContext;

        public UserController(
            IMailService mailService,
            IInviteService inviteService,
            IJwtService jwtService,
            IUserService userService,
            IConfiguration configuration,
            UserManager<TBoardUser> userManager,
            SignInManager<TBoardUser> signInManager,
            TBoardDbContext dbContext)
        {
            _mailService = mailService;
            _inviteService = inviteService;
            _jwtService = jwtService;
            _userService = userService;
            Configuration = configuration;

            _userManager = userManager;
            _signInManager = signInManager;

            _dbContext = dbContext;
        }

        [HttpPost("signIn")]
        [ProducesResponseType(typeof(UserDto), 200)]
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

                var expireMinute = Convert.ToInt32(Configuration["Jwt:ExpireMinute"]);
                Response.Cookies.Append(
                    "X-Access-Token",
                    token,
                    new CookieOptions()
                    {
                        HttpOnly = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTime.Now.AddMinutes(expireMinute),
                        Secure = true
                    });

                var userRoles = await _userManager.GetRolesAsync(user);
                var companyUser = await _dbContext.CompanyUsers.FirstOrDefaultAsync(cu => cu.UserId == user.Id);

                var userInfo = new UserDto()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Title = user.Title,
                    PhoneNumber = user.PhoneNumber,
                    Roles = userRoles.ToArray(),
                    CompanyId = companyUser?.CompanyId
                };

                return Ok(userInfo);
            }

            if (result.IsLockedOut) return Forbid("Account is locked!");

            return BadRequest("Check credentials!");
        }

        [HttpGet("isAuth")]
        [ProducesResponseType(typeof(UserDto), 200)]
        public async Task<IActionResult> IsAuth()
        {
            var isAuth = await HttpContext.IsAuth();
            if (isAuth is false)
                return Unauthorized();

            var userId = await HttpContext.GetCurrentUserId();
            var user = await _dbContext.BoardUsers.FirstOrDefaultAsync(u => u.Id == userId);
            var userRoles = await _userManager.GetRolesAsync(user);
            var companyUser = await _dbContext.CompanyUsers.FirstOrDefaultAsync(cu => cu.UserId == user.Id);

            return Ok(new UserDto()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Title = user.Title,
                PhoneNumber = user.PhoneNumber,
                Roles = userRoles.ToArray(),
                CompanyId = companyUser?.CompanyId
            });
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

        [Authorize(Roles = "Admin, CompanyOwner")]
        [HttpPost("disableUser")]
        public async Task<IActionResult> DisableUser([FromQuery] string email)
        {
            return await SetUserLockout(email, true);
        }

        [Authorize(Roles = "Admin, CompanyOwner")]
        [HttpPost("enableUser")]
        public async Task<IActionResult> EnableUser([FromQuery] string email)
        {
            return await SetUserLockout(email, false);
        }

        private async Task<IActionResult> SetUserLockout(string userEmail, bool locked)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) NotFound();
            var userRoles = await _userManager.GetRolesAsync(user);

            var userIsAdminOrCompanyOwner = userRoles.Contains("Admin") || userRoles.Contains("CompanyOwner");

            if (HttpContext.IsCurrentUserAdmin() is false && userIsAdminOrCompanyOwner) return Forbid();

            var disabled = await _userManager.SetLockoutEnabledAsync(user, locked);
            if (disabled.Succeeded is false) return UnprocessableEntity(disabled.Errors);

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

        [HttpPost("setPassword")]
        public async Task<IActionResult> SetPassword([FromQuery(Name = "c")] string inviteCode, [FromBody] SetPasswordRequest setPasswordRequest)
        {
            if (ModelState.IsValid is false) return BadRequest(setPasswordRequest);
            if (string.IsNullOrEmpty(inviteCode)) return BadRequest();

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

        [Authorize]
        [HttpPost("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] SetPasswordRequest setPasswordRequest)
        {
            var userId = await HttpContext.GetCurrentUserId();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            if (string.Equals(setPasswordRequest.Password, setPasswordRequest.ConfirmPassword) is false) return BadRequest("Passwords does not match!");

            var result = await _userManager.ChangePasswordAsync(user, setPasswordRequest.Password, setPasswordRequest.ConfirmPassword);
            if (result.Succeeded is false) return Problem("Password could not changed!");

            return Ok();
        }

        [Authorize]
        [HttpPost("changeAvatar")]
        public async Task<IActionResult> ChangeAvatar(string avatar)
        {
            var userId = await HttpContext.GetCurrentUserId();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            user.AvatarURL = avatar;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded is false) return Problem("Avatar could not changed!");

            return Ok();
        }

        [HttpPost("sendPasswordResetMail")]
        public async Task<IActionResult> SendPasswordResetMail([FromQuery(Name = "e")] string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Ok();

            var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _mailService.SendMail(new t_board.Services.Models.MailModel()
            {
                To = email,
                Subject = "Board Reset Password",
                Body = "https://board.com.tr/user/resetPassword?t=" + HttpUtility.UrlEncode(passwordResetToken) + "&e=" + email
            }, false);

            return Ok();
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromQuery(Name = "t")] string passwordResetToken, [FromQuery(Name = "e")] string email, [FromBody] SetPasswordRequest setPasswordRequest)
        {
            if (string.IsNullOrEmpty(passwordResetToken)) return BadRequest();
            if (string.IsNullOrEmpty(email)) return BadRequest();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) BadRequest();

            if (string.Equals(setPasswordRequest.Password, setPasswordRequest.ConfirmPassword) is false) return BadRequest("Passwords does not match!");

            var result = await _userManager.ResetPasswordAsync(user, HttpUtility.UrlDecode(passwordResetToken), setPasswordRequest.Password);
            if (result.Succeeded is false) return Problem("Password could not reset!");

            return Ok();
        }

        [HttpPost("signOut")]
        public async Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete("X-Access-Token");
            return Ok();
        }
    }
}
