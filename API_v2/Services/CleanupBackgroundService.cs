using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using API_v2.Repositories.IRepositories;

namespace API_v2.Services
{
    public class CleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CleanupBackgroundService> _logger;

        public CleanupBackgroundService(IServiceProvider serviceProvider, ILogger<CleanupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cleanup Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting database cleanup task...");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var tokenRepo = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
                        var notifRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                        var otpRepo = scope.ServiceProvider.GetRequiredService<IOtpRepository>();
                        var cutoff = DateTime.UtcNow.AddMonths(-1);

                        // Cleanup old RefreshTokens
                        await tokenRepo.DeleteExpiredTokensAsync(cutoff);

                        // Cleanup old Notifications
                        await notifRepo.DeleteOldNotificationsAsync(cutoff);

                        // OTP records are short-lived; retain expired challenges for
                        // at most 24 hours for operational troubleshooting.
                        await otpRepo.DeleteExpiredOtpsAsync(DateTime.UtcNow.AddHours(-24));

                        _logger.LogInformation("Database cleanup completed. Cleaned up records older than 1 month.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during database cleanup in CleanupBackgroundService.");
                }

                // Wait 24 hours
                try
                {
                    await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("Cleanup Background Service is stopping.");
        }
    }
}
