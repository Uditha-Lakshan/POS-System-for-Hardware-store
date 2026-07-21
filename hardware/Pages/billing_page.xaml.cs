using HardwareShop.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HardwareShop.Pages
{
    public partial class BillingPage : Page
    {
        // ObservableCollection automatically updates the DataGrid when items are added
        private ObservableCollection<BillItem> _cart = new ObservableCollection<BillItem>();
        private ShopDbContext _db = new ShopDbContext(); // Your database connection

        public BillingPage()
        {
            InitializeComponent();
            dgClientBill.ItemsSource = _cart;
            LoadData();
        }

        private void LoadData()
        {
            dgInventory.ItemsSource = _db.Items.ToList();
            cmbCustomers.ItemsSource = _db.Customers.ToList();
        }

        private void BtnAddToBill_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs
            if (decimal.TryParse(txtPrice.Text, out decimal price) &&
               int.TryParse(txtQty.Text, out int qty) &&
               decimal.TryParse(txtDiscount.Text, out decimal discount))
            {
                // Create new bill item
                var newItem = new BillItem
                {
                    ItemName = txtItemSearch.Text,
                    Price = price,
                    Qty = qty,
                    AppliedDiscount = discount
                };

                _cart.Add(newItem);
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            decimal total = _cart.Sum(item => item.Total);
            txtTotal.Text = $"Total: ${total:F2}";
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCustomers.SelectedItem is Customer selectedCustomer)
            {
                decimal totalToCharge = _cart.Sum(i => i.Total);

                // DEBT SYSTEM LOGIC
                if (selectedCustomer.CurrentDebt + totalToCharge > selectedCustomer.CreditLimit)
                {
                    MessageBox.Show("Transaction failed. This exceeds the customer's credit limit.");
                    return;
                }

                // Add to customer debt
                selectedCustomer.CurrentDebt += totalToCharge;
                _db.SaveChanges(); // Save to database

                MessageBox.Show($"Bill charged to {selectedCustomer.Name}. New Debt: ${selectedCustomer.CurrentDebt}");
                _cart.Clear();
                UpdateTotal();
            }
        }
    }
}