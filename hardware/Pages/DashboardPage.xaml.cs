using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HardwareShop.Models;

namespace HardwareShop.Pages
{
    public partial class DashboardPage : Page
    {
        private ShopDbContext _db = new();

        public DashboardPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Calculate total stock safely in C#
            int totalItems = _db.Items
                               .AsEnumerable()
                               .Sum(i => i.ItStock);

            // Count customers
            int totalCustomers = _db.Customers.Count();

            // Calculate total debt safely using .AsEnumerable() to prevent SQLite decimal errors
            decimal totalDebt = _db.Customers
                                   .AsEnumerable()
                                   .Sum(c => c.CurrentDebt);

            // Update UI elements
            txtTotalItems.Text = totalItems.ToString();
            txtTotalCustomers.Text = totalCustomers.ToString();
            txtTotalDebt.Text = $"${totalDebt:F2}";
        }
    }
}