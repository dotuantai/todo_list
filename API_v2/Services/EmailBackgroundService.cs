using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IEmailQueue _emailQueue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(
            IEmailQueue emailQueue,
            IServiceProvider serviceProvider,
            ILogger<EmailBackgroundService> logger)
        {
            _emailQueue = emailQueue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var emailItem = await _emailQueue.DequeueAsync(stoppingToken);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        try
                        {
                            _logger.LogInformation("Sending email from background queue to {Email}", emailItem.ToEmail);
                            emailService.SendEmail(emailItem.ToEmail, emailItem.Subject, emailItem.Body);
                            _logger.LogInformation("Successfully sent email from background queue to {Email}", emailItem.ToEmail);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error occurred sending email from queue to {Email}", emailItem.ToEmail);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Stopping token was triggered
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing EmailBackgroundService.");
                }
            }

            _logger.LogInformation("Email Background Service is stopping.");
        }
    }
}
