using Microsoft.EntityFrameworkCore;

namespace Sales.Data
{
    public class SalesDataContext(DbContextOptions<SalesDataContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(order =>
            {
                order.HasKey(o => o.OrderId);
                order.Property(o => o.OrderId).HasMaxLength(8);
            });
        }
    }
}
