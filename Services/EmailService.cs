using System.Net.Mail;
using System.Net;

namespace LearningManagementSystem.Services
{
    public class EmailService
    {
        private readonly string _smtpHost = "smtp.gmail.com"; 
        private readonly int _smtpPort = 465;
        private readonly string _senderEmail = "yara.emad4869@gmail.com"; 
        private readonly string _senderPassword = "einp wvgz mgvc bisk"; 

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_senderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true 
            };
            message.To.Add(recipientEmail);

            using (var smtpClient = new SmtpClient(_smtpHost, _smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
                smtpClient.EnableSsl = true;
                await smtpClient.SendMailAsync(message);
            }
        }
    }
}
