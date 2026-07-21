using System;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            var gmailAddress = _config["GmailSettings:GmailAddress"]?.Trim();
            var appPassword = _config["GmailSettings:AppPassword"]?.Replace(" ", "").Trim();

            if (string.IsNullOrEmpty(gmailAddress) || string.IsNullOrEmpty(appPassword))
            {
                _logger.LogError("Gmail settings are not configured properly in appsettings.");
                throw new InvalidOperationException("Email settings are not configured.");
            }

            try
            {
                using var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(gmailAddress, "TaskFlow Pro");
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = true;

                using var smtpClient = new SmtpClient("smtp.gmail.com", 587);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(gmailAddress, appPassword);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                smtpClient.Send(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }
    }
}
