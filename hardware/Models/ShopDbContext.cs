using Microsoft.EntityFrameworkCore;

namespace HardwareShop.Models
{
    public class ShopDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<BillItem> BillItems { get; set; }

        // THIS LINE AUTOMATICALLY BUILDS THE TABLES
        public ShopDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=hardware.db");
        }
    }
}