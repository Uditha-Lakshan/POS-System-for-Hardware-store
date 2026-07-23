using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HardwareShop.Pages;

namespace HardwareShop
{
    public partial class MainWindow : Window
    {
        private Brush _activeBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0D9488"));
        private Brush _transparentBrush = Brushes.Transparent;
        private Brush _activeTextBrush = Brushes.White;
        private Brush _inactiveTextBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));

        public MainWindow()
        {
            InitializeComponent();
            NavigateTo(new BillingPage(), btnNavBilling, "Billing & Point of Sale");
        }

        private void SetActiveButton(Button activeButton)
        {
            Button[] buttons = new[] { btnNavBilling, btnNavItems, btnNavCategories, btnNavCustomer, btnNavDashboard };
            foreach (var btn in buttons)
            {
                if (btn != null)
                {
                    btn.Background = _transparentBrush;
                    btn.Foreground = _inactiveTextBrush;
                }
            }

            if (activeButton != null)
            {
                activeButton.Background = _activeBrush;
                activeButton.Foreground = _activeTextBrush;
            }
        }

        private void NavigateTo(Page page, Button navButton, string headerTitle)
        {
            MainFrame.Navigate(page);
            SetActiveButton(navButton);
            if (txtPageHeader != null)
            {
                txtPageHeader.Text = headerTitle;
            }
        }

        private void Nav_Items_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new ItemsPage(), btnNavItems, "Items Management");
        }

        private void Nav_Categories_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new CategoriesPage(), btnNavCategories, "Category Management");
        }

        private void Nav_Customer_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new CustomerPage(), btnNavCustomer, "Customer & Credit Management");
        }

        private void Nav_Billing_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new BillingPage(), btnNavBilling, "Billing & Point of Sale");
        }

        private void Nav_Dashboard_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new DashboardPage(), btnNavDashboard, "Store Analytics & Dashboard");
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}