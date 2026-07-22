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

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            // 1. Create a new instance of the Login Window
            LoginWindow loginWindow = new LoginWindow();

            // 2. Show the Login Window
            loginWindow.Show();

            // 3. Close the current Main Window
            this.Close();
        }
    }
}