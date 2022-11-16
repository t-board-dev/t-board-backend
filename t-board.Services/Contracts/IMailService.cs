using System.Threading.Tasks;
using t_board.Services.Models;

namespace t_board.Services.Contracts
{
    public interface IMailService
    {
        Task SendMail(MailModel mailModel, bool isHtml);
    }
}
