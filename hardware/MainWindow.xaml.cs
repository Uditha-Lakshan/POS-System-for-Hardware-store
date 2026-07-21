using System.Windows;
using HardwareShop.Pages;

namespace HardwareShop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Using the correct class name
            MainFrame.Navigate(new BillingPage());
        }

        private void Nav_Items_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ItemsPage());
        }

        private void Nav_Categories_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CategoriesPage());
        }

        private void Nav_Customer_Click(object sender, RoutedEventArgs e)
        {
            // Using the correct class name
            MainFrame.Navigate(new CustomerPage());
        }

        private void Nav_Billing_Click(object sender, RoutedEventArgs e)
        {
            // Using the correct class name
            MainFrame.Navigate(new BillingPage());
        }

        private void Nav_Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DashboardPage());
        }
    }
}