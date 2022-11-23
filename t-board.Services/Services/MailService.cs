using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
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
            ValidateMailModel(mailModel);

            var apiKey = Configuration["Mail:SENDGRID_KEY"];
            var client = new SendGridClient(apiKey);

            var mailFrom = Configuration["Mail:MAIL_FROM"];
            var from = new EmailAddress(mailFrom);
            var to = new EmailAddress(mailModel.To);

            var email = isHtml ?
                MailHelper.CreateSingleEmail(from, to, mailModel.Subject, mailModel.Subject, string.Empty) :
                MailHelper.CreateSingleEmail(from, to, mailModel.Subject, string.Empty, mailModel.Subject);

            var response = await client.SendEmailAsync(email);

            if (response.IsSuccessStatusCode is false)
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                throw new($"Mail could not send! {responseBody}");
            }
        }

        private void ValidateMailModel(MailModel mailModel)
        {
            if (string.IsNullOrEmpty(mailModel.To))
                throw new ArgumentNullException("to");

            if (string.IsNullOrEmpty(mailModel.Subject))
                throw new ArgumentNullException("subject");

            if (string.IsNullOrEmpty(mailModel.Body))
                throw new ArgumentNullException("body");
        }
    }
}
