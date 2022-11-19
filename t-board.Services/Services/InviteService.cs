using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using t_board.Entity;
using t_board.Services.Contracts;
using t_board.Services.Models;

namespace t_board.Services.Services
{
    public sealed class InviteService : IInviteService
    {
        private readonly IMailService _mailService;

        private readonly TBoardDbContext _dbContext;

        public InviteService(
            IMailService mailService,
            TBoardDbContext dbcontext)
        {
            _mailService = mailService;

            _dbContext = dbcontext;
        }

        public async Task<(bool Succeeded, string Message)> SendInvitation(string userEmail)
        {
            var invitation = await _dbContext.UserInvitations.Where(i => i.UserEmail == userEmail).FirstOrDefaultAsync();

            if (invitation == null)
            {
                invitation = new UserInvitation
                {
                    UserEmail = userEmail,
                    InviteCode = Guid.NewGuid().ToString()
                };

                _dbContext.Add(invitation);
            }

            if (invitation.IsConfirmed) return (false, $"User with the mail '{userEmail}' has already confirmed the invitation!");

            invitation.InviteDate = DateTime.Now;
            invitation.ExpireDate = DateTime.Today.AddDays(2);

            await _dbContext.SaveChangesAsync();

            await _mailService.SendMail(new MailModel()
            {
                Subject = "T-Board Invitation",
                Body = invitation.InviteCode,
                To = userEmail
            },
            false);

            return (true, $"Invitation has been sent for the user with the mail {userEmail}!");
        }
    }
}
