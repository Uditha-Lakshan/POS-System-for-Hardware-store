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

        // This runs every time the dashboard is clicked, ensuring numbers are always up to date
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            int totalItems = _db.Items.Sum(i => i.ItStock);
            int totalCustomers = _db.Customers.Count();
            decimal totalDebt = _db.Customers.Sum(c => c.CurrentDebt);

            txtTotalItems.Text = totalItems.ToString();
            txtTotalCustomers.Text = totalCustomers.ToString();
            txtTotalDebt.Text = $"${totalDebt:F2}";
        }
    }
}