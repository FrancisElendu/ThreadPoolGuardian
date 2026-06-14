using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using ThreadPoolGuardian.Models;

namespace ThreadPoolGuardian.Services
{
    public class AppInsightsMetricsStorage : IThreadPoolMetricsStorage
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<AppInsightsMetricsStorage> _logger;

        public AppInsightsMetricsStorage(
            TelemetryClient telemetryClient,
            ILogger<AppInsightsMetricsStorage> logger)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        public async Task StoreMetricsAsync(ThreadPoolMetrics metrics)
        {
            try
            {
                // Track as metrics (for aggregation)
                _telemetryClient.GetMetric("ThreadPool.PendingWorkItems").TrackValue(metrics.PendingWorkItems);
                _telemetryClient.GetMetric("ThreadPool.UsedThreads").TrackValue(metrics.UsedThreads);
                _telemetryClient.GetMetric("ThreadPool.AvailableThreads").TrackValue(metrics.AvailableThreads);
                _telemetryClient.GetMetric("ThreadPool.UtilizationPercent").TrackValue(metrics.UtilizationPercent);

                // Track as custom event - properties only (no separate measurements)
                var properties = new Dictionary<string, string>
                {
                    { "Status", metrics.Status },
                    { "Timestamp", metrics.Timestamp.ToString("o") },
                    { "PendingWorkItems", metrics.PendingWorkItems.ToString() },
                    { "UsedThreads", metrics.UsedThreads.ToString() },
                    { "AvailableThreads", metrics.AvailableThreads.ToString() },
                    { "MaxThreads", metrics.MaxThreads.ToString() },
                    { "UtilizationPercent", metrics.UtilizationPercent.ToString() }
                };

                _telemetryClient.TrackEvent("ThreadPoolHealthCheck", properties);

                // Track as trace for log search
                _telemetryClient.TrackTrace(
                    $"ThreadPool Status: {metrics.Status} | Pending: {metrics.PendingWorkItems} | Utilization: {metrics.UtilizationPercent}%",
                    GetSeverityLevel(metrics.Status));

                await Task.CompletedTask; // AI is async-free

                _logger.LogDebug("Stored ThreadPool metrics to Application Insights");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store ThreadPool metrics to Application Insights");
                // Don't throw - monitoring shouldn't crash the app
            }
        }

        private SeverityLevel GetSeverityLevel(string status)
        {
            return status switch
            {
                "CRITICAL" => SeverityLevel.Critical,
                "WARNING" => SeverityLevel.Warning,
                _ => SeverityLevel.Information
            };
        }
    }
}
