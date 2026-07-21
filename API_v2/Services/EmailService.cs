using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly HttpClient _httpClient;

        public EmailService(IConfiguration config, ILogger<EmailService> logger, HttpClient httpClient)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClient;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            SendEmailAsync(toEmail, subject, body).GetAwaiter().GetResult();
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var apiKey = _config["BrevoSettings:ApiKey"]?.Trim();
            var senderEmail = _config["BrevoSettings:SenderEmail"]?.Trim() ?? "tai.do@rikai.technology";
            var senderName = _config["BrevoSettings:SenderName"]?.Trim() ?? "TaskFlow Pro";

            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_BREVO_API_KEY_HERE")
            {
                _logger.LogError("Brevo API Key is not configured. Please set BrevoSettings:ApiKey.");
                throw new InvalidOperationException("Email settings are not configured.");
            }

            var requestBody = new
            {
                sender = new { name = senderName, email = senderEmail },
                to = new[] { new { email = toEmail } },
                subject = subject,
                htmlContent = body
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            request.Headers.Add("api-key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = httpContent;

            try
            {
                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorDetails = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send email via Brevo API. Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorDetails);
                    throw new HttpRequestException($"Brevo API error: {response.StatusCode}");
                }

                _logger.LogInformation("Email sent successfully via Brevo API to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }
    }
}
