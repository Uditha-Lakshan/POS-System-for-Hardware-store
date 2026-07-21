using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HardwareShop.Models;

namespace HardwareShop.Pages
{
    public partial class CustomerPage : Page
    {
        private ShopDbContext _db = new ShopDbContext();

        public CustomerPage()
        {
            InitializeComponent();
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            // Pulls the latest customer list from the database
            dgCustomers.ItemsSource = _db.Customers.ToList();
        }

        private void BtnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            // Verify numbers are typed in correctly
            if (decimal.TryParse(txtDiscount.Text, out decimal discount) &&
                decimal.TryParse(txtCreditLimit.Text, out decimal creditLimit))
            {
                var newCustomer = new Customer
                {
                    Name = txtName.Text,
                    Phone = txtPhone.Text,
                    CustomerLoyaltyDiscount = discount,
                    CreditLimit = creditLimit,
                    CurrentDebt = 0 // Starts with 0 debt
                };

                _db.Customers.Add(newCustomer);
                _db.SaveChanges(); // Save to database
                LoadCustomers();   // Refresh the grid

                // Clear the text boxes
                txtName.Clear();
                txtPhone.Clear();
                txtDiscount.Clear();
                txtCreditLimit.Clear();
            }
            else
            {
                MessageBox.Show("Please enter valid numbers for Discount and Credit Limit.");
            }
        }

        private void BtnSettleDebt_Click(object sender, RoutedEventArgs e)
        {
            // Check if a customer is selected in the data grid
            if (dgCustomers.SelectedItem is Customer selectedCustomer)
            {
                if (decimal.TryParse(txtPaymentAmount.Text, out decimal payment))
                {
                    selectedCustomer.CurrentDebt -= payment;

                    // Don't let debt go below 0
                    if (selectedCustomer.CurrentDebt < 0)
                        selectedCustomer.CurrentDebt = 0;

                    _db.Customers.Update(selectedCustomer);
                    _db.SaveChanges(); // Update database
                    LoadCustomers();

                    txtPaymentAmount.Clear();
                    MessageBox.Show($"Payment of ${payment} applied. Remaining Debt: ${selectedCustomer.CurrentDebt:F2}");
                }
            }
            else
            {
                MessageBox.Show("Please click on a customer in the list first to settle their debt.");
            }
        }
    }
}