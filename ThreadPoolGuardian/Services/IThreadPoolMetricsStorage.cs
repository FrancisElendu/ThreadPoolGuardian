using ThreadPoolGuardian.Models;

namespace ThreadPoolGuardian.Services
{
    public interface IThreadPoolMetricsStorage
    {
        Task StoreMetricsAsync(ThreadPoolMetrics metrics);
    }
}
