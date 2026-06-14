using ThreadPoolGuardian.Models;

namespace ThreadPoolGuardian.Services
{
    // Composite storage class
    public class CompositeStorage : IThreadPoolMetricsStorage
    {
        private readonly IEnumerable<IThreadPoolMetricsStorage> _storages;

        public CompositeStorage(params IThreadPoolMetricsStorage[] storages)
        {
            _storages = storages;
        }

        public async Task StoreMetricsAsync(ThreadPoolMetrics metrics)
        {
            foreach (var storage in _storages)
            {
                await storage.StoreMetricsAsync(metrics);
            }
        }
    }
}
