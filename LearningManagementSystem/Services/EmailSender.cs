using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
namespace LearningManagementSystem.Services
{
public class EmailSender : IEmailSender
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;

        public EmailSender(string smtpHost, int smtpPort, string smtpUser, string smtpPass)
        {
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _smtpUser = smtpUser;
            _smtpPass = smtpPass;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using (var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                EnableSsl = true
            })
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUser),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
        }
    }

}
