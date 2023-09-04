using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace IdentityService.Services
{
    public class EmailSenderService : IEmailSender
    {
    private readonly IConfiguration _configuration;

    public EmailSenderService(IConfiguration configuration)
        {
      this._configuration = configuration;
    }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var mailSettings = _configuration.GetSection("MailSettings");
            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress(mailSettings["FromName"], mailSettings["FromEmail"]));
            mailMessage.To.Add(new MailboxAddress(email, email));
            mailMessage.Subject = subject;
            mailMessage.Body = new TextPart("html")
            {
                Text = htmlMessage
            };

            using (var smtpClient = new SmtpClient())
            {
                var mailSettingsSmtp = mailSettings.GetSection("SmtpSettings");
                smtpClient.Connect(mailSettingsSmtp["Host"], Int32.Parse(mailSettingsSmtp["Port"]), true);
                smtpClient.Authenticate(mailSettingsSmtp["UserName"], mailSettingsSmtp["Password"]);
                smtpClient.Send(mailMessage);
                smtpClient.Disconnect(true);
            }

            return Task.CompletedTask;
        }
    }
}
