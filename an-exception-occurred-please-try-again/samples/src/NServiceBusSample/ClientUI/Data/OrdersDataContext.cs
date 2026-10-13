using Microsoft.EntityFrameworkCore;

namespace ClientUI.Data
{
    public class OrdersDataContext(DbContextOptions<OrdersDataContext> options) : DbContext(options)
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
