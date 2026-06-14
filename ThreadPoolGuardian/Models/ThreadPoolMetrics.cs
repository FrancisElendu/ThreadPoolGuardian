namespace ThreadPoolGuardian.Models
{
    public class ThreadPoolMetrics
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int PendingWorkItems { get; set; }
        public int UsedThreads { get; set; }
        public int AvailableThreads { get; set; }
        public int MaxThreads { get; set; }
        public double UtilizationPercent { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AdditionalData { get; set; } // For JSON extras
    }
}
