﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using t_board.Entity;
using t_board.Entity.Entity;
using t_board_backend.Models.User;

namespace t_board_backend.Controllers
{
    [Authorize]
    [Route("user/")]
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
        [HttpPost("signIn")]
        public async Task<IActionResult> SignIn(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null) return NotFound(email);

            var canSignIn = await _signInManager.CanSignInAsync(user);
            var checkPassword = await _signInManager.CheckPasswordSignInAsync(user, password, false);

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, false, false);
            if (result.Succeeded)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("email", user.Email),
                        new Claim("firstName", user.FirstName),
                        new Claim("lastName", user.LastName),
                        new Claim("roles", JsonSerializer.Serialize(userRoles))
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(60),
                    signingCredentials: signIn);

                return Ok(new JwtSecurityTokenHandler().WriteToken(token));
            }

            if (result.IsLockedOut) return Forbid("Account is locked!");

            return BadRequest("Check credentials!");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("createUser")]
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

            if (created.Succeeded is false) return UnprocessableEntity(created.Errors);

            var invitationSent = await SendInvitationCore(user.Email);
            if (invitationSent.Succeeded is false) return UnprocessableEntity(invitationSent.Message);

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("sendInvitation")]
        public async Task<IActionResult> SendInvitation(string userEmail)
        {
            var invitationSent = await SendInvitationCore(userEmail);

            if (invitationSent.Succeeded is false) return UnprocessableEntity(invitationSent.Message);

            return Ok();
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
        [HttpPost("checkInvitation")]
        public async Task<IActionResult> CheckInvitation(string inviteCode)
        {
            var invitation = await _dbContext.UserInvitations
                .Where(i => i.InviteCode == inviteCode)
                .FirstOrDefaultAsync();

            if (invitation == null) return NotFound(inviteCode);
            if (invitation.IsConfirmed) return Conflict("Already confirmed!");
            if (invitation.ExpireDate < DateTime.Now) Conflict("Invitation has expired!");

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("setPassword")]
        public async Task<IActionResult> SetPassword(string inviteCode, string password)
        {
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

            var passwordAdded = await _userManager.AddPasswordAsync(user, password);
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
