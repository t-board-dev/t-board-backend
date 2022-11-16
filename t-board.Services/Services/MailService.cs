using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using t_board.Services.Contracts;
using t_board.Services.Models;

namespace t_board.Services.Services
{
    public sealed class MailService : IMailService
    {
        private readonly IConfiguration Configuration;

        public MailService(
            IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task SendMail(MailModel mailModel, bool isHtml)
        {
            if (string.IsNullOrEmpty(mailModel.To))
                throw new ArgumentNullException("to");

            if (string.IsNullOrEmpty(mailModel.Subject))
                throw new ArgumentNullException("subject");

            if (string.IsNullOrEmpty(mailModel.Body))
                throw new ArgumentNullException("body");

            var username = Configuration["Smtp:Username"];
            var password = Configuration["Smtp:Password"];

            var credentials = new NetworkCredential(username, password);

            var server = Configuration["Smtp:ServerURL"];
            var port = Convert.ToInt32(Configuration["Smtp:Port"]);

            var smtpClient = new SmtpClient(server)
            {
                Port = port,
                Credentials = credentials,
                EnableSsl = false,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username),
                Subject = mailModel.Subject,
                Body = mailModel.Body,
                IsBodyHtml = isHtml,
            };

            mailMessage.To.Add(mailModel.To);

            if (string.IsNullOrEmpty(mailModel.Cc) is false)
                mailMessage.CC.Add(mailModel.Cc);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
