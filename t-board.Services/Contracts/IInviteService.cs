using System.Threading.Tasks;

namespace t_board.Services.Contracts
{
    public interface IInviteService
    {
        Task<(bool Succeeded, string Message)> SendInvitation(string userEmail);
    }
}
