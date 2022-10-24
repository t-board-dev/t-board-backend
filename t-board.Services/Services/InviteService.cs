using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using t_board.Entity;
using t_board.Services.Contracts;

namespace t_board.Services.Services
{
    public sealed class InviteService : IInviteService
    {
        private readonly TBoardDbContext _dbContext;

        public InviteService(
            TBoardDbContext dbcontext)
        {
            _dbContext = dbcontext;
        }

        public async Task<(bool Succeeded, string Message)> SendInvitation(string userEmail)
        {
            return await SendInvitationCore(userEmail);
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
                if (invitation.IsConfirmed) return (false, $"User with the mail '{userEmail}' has already confirmed the invitation!");

                invitation.InviteDate = DateTime.Now;
                invitation.ExpireDate = DateTime.Today.AddDays(2);
            }

            await _dbContext.SaveChangesAsync();

            // TODO:
            // Send invitation

            return (true, $"Invitation has been sent for the user with the mail {userEmail}!");
        }
    }
}
