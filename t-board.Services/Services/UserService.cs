using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using t_board.Entity;
using t_board.Services.Contracts;

namespace t_board.Services.Services
{
    public sealed class UserService : IUserService
    {
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
    }
}
