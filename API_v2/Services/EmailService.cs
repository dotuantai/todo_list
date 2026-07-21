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
            var googleScriptUrl = _config["GoogleScriptSettings:Url"]?.Trim();

            if (string.IsNullOrEmpty(googleScriptUrl) || googleScriptUrl.Contains("YOUR_"))
            {
                _logger.LogError("Google Script URL is not configured properly in appsettings.");
                throw new InvalidOperationException("Email settings are not configured.");
            }

            var requestBody = new
            {
                to = toEmail,
                subject = subject,
                html = body
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);

            // Google Apps Script redirects POST requests with HTTP 302 Found.
            // By default, HttpClient automatically redirects but changes the method from POST to GET.
            // To preserve POST, we handle the 302 redirect manually.
            var currentUrl = googleScriptUrl;
            int maxRedirects = 5;
            HttpResponseMessage? response = null;

            for (int i = 0; i < maxRedirects; i++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, currentUrl);
                using var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                request.Content = httpContent;
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                response = await _httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.Redirect ||
                    response.StatusCode == System.Net.HttpStatusCode.Found ||
                    response.StatusCode == System.Net.HttpStatusCode.SeeOther ||
                    response.StatusCode == System.Net.HttpStatusCode.TemporaryRedirect)
                {
                    Uri? redirectUrl = response.Headers.Location;
                    if (redirectUrl != null)
                    {
                        if (!redirectUrl.IsAbsoluteUri)
                        {
                            redirectUrl = new Uri(new Uri(currentUrl), redirectUrl);
                        }
                        currentUrl = redirectUrl.ToString();
                        response.Dispose(); // Dispose intermediate response before redirecting
                        continue;
                    }
                }
                break;
            }

            try
            {
                if (response == null || !response.IsSuccessStatusCode)
                {
                    var errorDetails = response != null ? await response.Content.ReadAsStringAsync() : "No response received";
                    _logger.LogError("Failed to send email via Google Script API. Status: {StatusCode}, Error: {Error}", 
                        response?.StatusCode, errorDetails);
                    throw new HttpRequestException($"Google Script API error: {response?.StatusCode}");
                }

                _logger.LogInformation("Email sent successfully via Google Script API to {Email}", toEmail);
            }
            finally
            {
                response?.Dispose();
            }
        }
    }
}
