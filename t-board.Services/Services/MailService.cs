﻿using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using t_board.Services.Contracts;
using t_board.Services.Models;

namespace t_board.Services.Services
{
    public sealed class MailService : IMailService
    {
        private readonly string _apiKey = Environment.GetEnvironmentVariable("SENDGRID_API");
        private readonly string _mailFrom = Environment.GetEnvironmentVariable("MAIL_FROM");

        public async Task SendMail(MailModel mailModel, bool isHtml)
        {
            ValidateMailModel(mailModel);

            var apiKey = _apiKey;
            var client = new SendGridClient(apiKey);

            var mailFrom = _mailFrom;
            var from = new EmailAddress(mailFrom);
            var to = new EmailAddress(mailModel.To);

            var email = isHtml ?
                MailHelper.CreateSingleEmail(from, to, mailModel.Subject, mailModel.Body, string.Empty) :
                MailHelper.CreateSingleEmail(from, to, mailModel.Subject, string.Empty, mailModel.Body);

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
