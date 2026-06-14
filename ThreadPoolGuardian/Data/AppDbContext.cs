using Microsoft.EntityFrameworkCore;
using ThreadPoolGuardian.Models;

namespace ThreadPoolGuardian.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public AppDbContext()
        {
        }
        public DbSet<ThreadPoolMetrics> ThreadPoolMetrics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ThreadPoolMetrics>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50);

                // Index for fast queries
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Status);
            });
        }
    }
}
