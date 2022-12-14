using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using t_board.Entity;

namespace t_board.Services.Contracts
{
    public interface IUserService
    {
        Task<(bool Succeeded, string Message)> CreateUser(TBoardUser user);
        Task<(bool Succeded, string Message)> AssignUserRole(string userEmail, string role);
        Task<IEnumerable<Claim>> GetUserClaims(TBoardUser user);
        Task<bool> LockUser(string email, DateTime? endDate = null);
        Task<bool> UnlockUser(string email);
        Task<bool> IsUserLocked(string email);
    }
}
