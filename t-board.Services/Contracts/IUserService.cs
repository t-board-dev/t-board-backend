using System.Threading.Tasks;
using t_board.Entity;

namespace t_board.Services.Contracts
{
    public interface IUserService
    {
        Task<(bool Succeeded, string Message)> CreateUser(TBoardUser user);
        Task<(bool Succeded, string Message)> AssignUserRole(string userEmail, string role);
    }
}
