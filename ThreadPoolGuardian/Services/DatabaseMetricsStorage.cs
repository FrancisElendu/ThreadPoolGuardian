using Microsoft.EntityFrameworkCore;
using ThreadPoolGuardian.Data;
using ThreadPoolGuardian.Models;

namespace ThreadPoolGuardian.Services
{
    public class DatabaseMetricsStorage : IThreadPoolMetricsStorage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseMetricsStorage> _logger;

        public DatabaseMetricsStorage(
            IServiceProvider serviceProvider,
            ILogger<DatabaseMetricsStorage> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StoreMetricsAsync(ThreadPoolMetrics metrics)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await dbContext.ThreadPoolMetrics.AddAsync(metrics);
                await dbContext.SaveChangesAsync();

                _logger.LogDebug("Stored ThreadPool metrics to database. Pending: {Pending}", metrics.PendingWorkItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store ThreadPool metrics to database");
                // Don't throw - monitoring shouldn't crash the app
            }
        }

        // Optional: Cleanup old records (keep last 30 days)
        public async Task CleanupOldRecordsAsync(int daysToKeep = 30)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                var oldRecords = await dbContext.ThreadPoolMetrics
                    .Where(m => m.Timestamp < cutoffDate)
                    .ToArrayAsync();

                dbContext.ThreadPoolMetrics.RemoveRange(oldRecords);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} old ThreadPool metrics records", oldRecords.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup old ThreadPool metrics");
            }
        }
    }
}
