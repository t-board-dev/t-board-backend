using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using t_board.Entity;
using t_board.Services.Contracts;

namespace t_board.Services.Services
{
    public sealed class UserService : IUserService
    {
        private readonly DateTime EndDate = new DateTime(2050, 01, 01);

        private readonly UserManager<TBoardUser> _userManager;

        private readonly TBoardDbContext _dbContext;

        public UserService(
            UserManager<TBoardUser> userManager,
            TBoardDbContext dbcontext)
        {
            _userManager = userManager;

            _dbContext = dbcontext;
        }

        public async Task<(bool Succeeded, string Message)> CreateUser(TBoardUser user)
        {
            var created = await _userManager.CreateAsync(user);
            return (created.Succeeded, string.Join(", ", created.Errors));
        }

        public async Task<(bool Succeded, string Message)> AssignUserRole(string userEmail, string role)
        {
            var user = await _userManager.FindByNameAsync(userEmail);
            if (user == null) return (false, $"User '{userEmail}' could not found!");

            var created = await _userManager.AddToRoleAsync(user, role);
            return (created.Succeeded, string.Join(", ", created.Errors));
        }

        public async Task<IEnumerable<Claim>> GetUserClaims(TBoardUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var userCompany = await _dbContext.CompanyUsers.Where(cu => cu.UserId == user.Id).Select(cu => cu.CompanyId).FirstOrDefaultAsync();

            var claims = new List<Claim> {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("id", user.Id),
                        new Claim("firstName", user.FirstName),
                        new Claim("lastName", user.LastName),
                        new Claim("email", user.Email),
                        new Claim("title", user.Title),
                        new Claim("company", userCompany.ToString()),
                        new Claim("phoneNumber", user.PhoneNumber)
                };

            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            return claims;
        }

        public async Task<bool> LockUser(string email, DateTime? endDate = null)
        {
            if (endDate == null) endDate = EndDate;

            var user = await _userManager.FindByEmailAsync(email);

            var lockUserResult = await _userManager.SetLockoutEnabledAsync(user, true);

            var lockDateResult = await _userManager.SetLockoutEndDateAsync(user, endDate);

            return lockDateResult.Succeeded && lockUserResult.Succeeded;
        }

        public async Task<bool> UnlockUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            var lockDisabledResult = await _userManager.SetLockoutEnabledAsync(user, false);

            var setLockoutEndDateResult = await _userManager.SetLockoutEndDateAsync(user, DateTime.Now - TimeSpan.FromMinutes(1));

            return setLockoutEndDateResult.Succeeded && lockDisabledResult.Succeeded;
        }

        public async Task<bool> IsUserLocked(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            
            return user.LockoutEnabled && user.LockoutEnd > DateTime.Now;
        }
    }
}
