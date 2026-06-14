using ThreadPoolGuardian.Models;

namespace ThreadPoolGuardian.Services
{
    public class ThreadPoolMonitorService : BackgroundService
    {
        private readonly ILogger<ThreadPoolMonitorService> _logger;
        private readonly IThreadPoolMetricsStorage _storage;

        public ThreadPoolMonitorService(
            ILogger<ThreadPoolMonitorService> logger,
            IThreadPoolMetricsStorage storage)
        {
            _logger = logger;
            _storage = storage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run every 10 minutes
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));

            // Run immediately on startup
            await CheckThreadPoolHealth();

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await CheckThreadPoolHealth();

                // If using Database storage, cleanup old records daily
                if (_storage is DatabaseMetricsStorage dbStorage)
                {
                    await dbStorage.CleanupOldRecordsAsync();
                }
            }
        }

        private async Task CheckThreadPoolHealth()
        {
            try
            {
                // Get ThreadPool metrics
                ThreadPool.GetAvailableThreads(out int availableWorkers, out int availableIo);
                ThreadPool.GetMaxThreads(out int maxWorkers, out int maxIo);

                // Get pending work items using reflection
                var pendingField = typeof(ThreadPool).GetField(
                    "_pendingWorkItemCount",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

                int pending = pendingField != null ? (int)(pendingField.GetValue(null) ?? 0) : -1;

                int usedWorkers = maxWorkers - availableWorkers;
                double utilizationPercent = (double)usedWorkers / maxWorkers * 100;

                // Determine status
                string status = pending > 1000 ? "CRITICAL" :
                               pending > 500 ? "WARNING" :
                               "HEALTHY";

                // Log to console/ILogger
                if (pending > 1000)
                {
                    _logger.LogCritical(
                        "[THREADPOOL] Status: {Status} | Pending: {Pending} | Utilization: {Utilization:F2}%",
                        status, pending, utilizationPercent);
                }
                else if (pending > 500)
                {
                    _logger.LogWarning(
                        "[THREADPOOL] Status: {Status} | Pending: {Pending} | Utilization: {Utilization:F2}%",
                        status, pending, utilizationPercent);
                }
                else
                {
                    _logger.LogInformation(
                        "[THREADPOOL] Status: {Status} | Pending: {Pending} | Utilization: {Utilization:F2}%",
                        status, pending, utilizationPercent);
                }

                // Store metrics
                var metrics = new ThreadPoolMetrics
                {
                    Timestamp = DateTime.UtcNow,
                    PendingWorkItems = pending,
                    UsedThreads = usedWorkers,
                    AvailableThreads = availableWorkers,
                    MaxThreads = maxWorkers,
                    UtilizationPercent = Math.Round(utilizationPercent, 2),
                    Status = status
                };

                await _storage.StoreMetricsAsync(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check ThreadPool health");
            }
        }
    }
}
