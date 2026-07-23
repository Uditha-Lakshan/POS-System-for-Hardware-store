using Microsoft.EntityFrameworkCore;

namespace HardwareShop.Models
{
    public class ShopDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<BillItem> BillItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<CustomerPayment> CustomerPayments { get; set; }

        public ShopDbContext()
        {
            Database.EnsureCreated();
            EnsureSchemaUpdated();
        }

        private void EnsureSchemaUpdated()
        {
            try
            {
                // Ensure SubCategories table exists
                Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS SubCategories (
                        SubCatCode INTEGER PRIMARY KEY AUTOINCREMENT,
                        SubCatName TEXT NOT NULL,
                        Description TEXT,
                        CategoryId INTEGER NOT NULL,
                        FOREIGN KEY (CategoryId) REFERENCES Categories (CatCode) ON DELETE CASCADE
                    );
                ");

                // Ensure Sales table exists
                Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS Sales (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SaleDate TEXT NOT NULL,
                        PaymentMethod TEXT,
                        SubTotal TEXT NOT NULL,
                        Discount TEXT NOT NULL,
                        TotalAmount TEXT NOT NULL,
                        CustomerId INTEGER,
                        CustomerName TEXT
                    );
                ");

                // Ensure SaleDetails table exists
                Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS SaleDetails (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SaleId INTEGER NOT NULL,
                        ItemId INTEGER NOT NULL,
                        ItemName TEXT,
                        UnitPrice TEXT NOT NULL,
                        Quantity INTEGER NOT NULL,
                        Discount TEXT NOT NULL,
                        TotalPrice TEXT NOT NULL,
                        FOREIGN KEY (SaleId) REFERENCES Sales (Id) ON DELETE CASCADE
                    );
                ");

                // Ensure CustomerPayments table exists
                Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS CustomerPayments (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CustomerId INTEGER NOT NULL,
                        CustomerName TEXT,
                        PaymentDate TEXT NOT NULL,
                        AmountPaid TEXT NOT NULL,
                        RemainingDebt TEXT NOT NULL,
                        PaymentMethod TEXT NOT NULL,
                        Notes TEXT
                    );
                ");

                // Try to add Unit column if missing in older database files
                try
                {
                    Database.ExecuteSqlRaw("ALTER TABLE Items ADD COLUMN Unit TEXT NOT NULL DEFAULT 'Pcs';");
                }
                catch { /* Column already exists */ }

                // Try to add SubCategoryId column to Items if missing
                try
                {
                    Database.ExecuteSqlRaw("ALTER TABLE Items ADD COLUMN SubCategoryId INTEGER;");
                }
                catch { /* Column already exists */ }

                // Try to add Description column to Categories if missing
                try
                {
                    Database.ExecuteSqlRaw("ALTER TABLE Categories ADD COLUMN Description TEXT NOT NULL DEFAULT '';");
                }
                catch { /* Column already exists */ }
            }
            catch { /* Database initialization fallback */ }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=hardware.db");
        }
    }
}