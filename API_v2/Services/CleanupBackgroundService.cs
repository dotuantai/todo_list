using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using API_v2.Datas;

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
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var cutoff = DateTime.UtcNow.AddMonths(-1);

                        // Cleanup old RefreshTokens
                        var oldTokens = db.RefreshTokens.Where(t => t.CreatedAt < cutoff);
                        db.RefreshTokens.RemoveRange(oldTokens);

                        // Cleanup old Notifications
                        var oldNotifications = db.Notifications.Where(n => n.CreatedAt < cutoff);
                        db.Notifications.RemoveRange(oldNotifications);

                        int deletedCount = db.SaveChanges();
                        _logger.LogInformation("Database cleanup completed. Cleaned up {Count} records older than 1 month.", deletedCount);
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
