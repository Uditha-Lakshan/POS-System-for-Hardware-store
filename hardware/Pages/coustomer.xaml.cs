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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _db = new ShopDbContext();
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            FilterCustomers();
        }

        private void FilterCustomers()
        {
            string filterText = txtSearchCustomer?.Text?.Trim()?.ToLower() ?? "";
            var query = _db.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterText))
            {
                query = query.Where(c => c.Name.ToLower().Contains(filterText) ||
                                         (c.Phone != null && c.Phone.Contains(filterText)));
            }

            dgCustomers.ItemsSource = query.ToList();
        }

        private void TxtSearchCustomer_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterCustomers();
        }

        private void DgCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCustomers.SelectedItem is Customer selectedCustomer)
            {
                txtName.Text = selectedCustomer.Name;
                txtPhone.Text = selectedCustomer.Phone;
                txtDiscount.Text = selectedCustomer.CustomerLoyaltyDiscount.ToString("F2");
                txtCreditLimit.Text = selectedCustomer.CreditLimit.ToString("F2");
                txtPaymentAmount.Text = selectedCustomer.CurrentDebt.ToString("F2");

                LoadPaymentHistory(selectedCustomer.Id, selectedCustomer.Name);
            }
            else
            {
                if (lblLedgerHeader != null) lblLedgerHeader.Text = "📜 Debt Payment History Log";
                if (dgPaymentHistory != null) dgPaymentHistory.ItemsSource = null;
            }
        }

        private void LoadPaymentHistory(int customerId, string customerName)
        {
            if (lblLedgerHeader != null) lblLedgerHeader.Text = $"📜 Payment History: {customerName}";

            var history = _db.CustomerPayments
                             .Where(p => p.CustomerId == customerId)
                             .OrderByDescending(p => p.PaymentDate)
                             .ToList();

            if (dgPaymentHistory != null) dgPaymentHistory.ItemsSource = history;
        }

        private void BtnAddCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a customer name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (decimal.TryParse(txtDiscount.Text, out decimal discount) && discount >= 0 &&
                decimal.TryParse(txtCreditLimit.Text, out decimal creditLimit) && creditLimit >= 0)
            {
                var newCustomer = new Customer
                {
                    Name = txtName.Text.Trim(),
                    Phone = txtPhone.Text?.Trim() ?? "",
                    CustomerLoyaltyDiscount = discount,
                    CreditLimit = creditLimit,
                    CurrentDebt = 0
                };

                _db.Customers.Add(newCustomer);
                _db.SaveChanges();
                LoadCustomers();
                ClearForm();

                MessageBox.Show($"Customer '{newCustomer.Name}' created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please enter valid positive numbers for Loyalty Discount % and Credit Limit.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnEditCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (dgCustomers.SelectedItem is Customer selectedCustomer)
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Please enter a customer name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (decimal.TryParse(txtDiscount.Text, out decimal discount) && discount >= 0 &&
                    decimal.TryParse(txtCreditLimit.Text, out decimal creditLimit) && creditLimit >= 0)
                {
                    selectedCustomer.Name = txtName.Text.Trim();
                    selectedCustomer.Phone = txtPhone.Text?.Trim() ?? "";
                    selectedCustomer.CustomerLoyaltyDiscount = discount;
                    selectedCustomer.CreditLimit = creditLimit;

                    _db.Customers.Update(selectedCustomer);
                    _db.SaveChanges();
                    LoadCustomers();
                    ClearForm();

                    MessageBox.Show($"Customer '{selectedCustomer.Name}' details updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Please enter valid positive numbers for Loyalty Discount % and Credit Limit.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please select a customer from the list to edit.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (dgCustomers.SelectedItem is Customer selectedCustomer)
            {
                string debtWarning = selectedCustomer.CurrentDebt > 0
                    ? $"\n\n⚠️ WARNING: This customer has an outstanding debt of ${selectedCustomer.CurrentDebt:F2}!"
                    : "";

                if (MessageBox.Show($"Are you sure you want to delete customer '{selectedCustomer.Name}'?{debtWarning}", "Confirm Delete Customer", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _db.Customers.Remove(selectedCustomer);
                    _db.SaveChanges();
                    LoadCustomers();
                    ClearForm();

                    MessageBox.Show("Customer account deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a customer from the list to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnClearCustomer_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtName.Clear();
            txtPhone.Clear();
            txtDiscount.Text = "0";
            txtCreditLimit.Text = "500";
            txtPaymentAmount.Clear();
            txtPaymentNotes.Clear();
            dgCustomers.SelectedItem = null;
            if (dgPaymentHistory != null) dgPaymentHistory.ItemsSource = null;
            if (lblLedgerHeader != null) lblLedgerHeader.Text = "📜 Debt Payment History Log";
        }

        private void BtnSettleDebt_Click(object sender, RoutedEventArgs e)
        {
            ProcessDebtPayment(false);
        }

        private void BtnSettleFullDebt_Click(object sender, RoutedEventArgs e)
        {
            ProcessDebtPayment(true);
        }

        private void ProcessDebtPayment(bool isFullSettlement)
        {
            if (dgCustomers.SelectedItem is Customer selectedCustomer)
            {
                if (selectedCustomer.CurrentDebt <= 0)
                {
                    MessageBox.Show($"{selectedCustomer.Name} does not have any outstanding debt balance.", "No Debt Pending", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                decimal paymentAmount = 0;
                if (isFullSettlement)
                {
                    paymentAmount = selectedCustomer.CurrentDebt;
                }
                else
                {
                    if (!decimal.TryParse(txtPaymentAmount.Text, out paymentAmount) || paymentAmount <= 0)
                    {
                        MessageBox.Show("Please enter a valid positive payment amount.", "Invalid Payment", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                string method = (cmbPaymentMethod.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Cash";
                string notes = txtPaymentNotes?.Text?.Trim() ?? (isFullSettlement ? "Full Debt Settlement" : "Partial Debt Settlement");

                decimal previousDebt = selectedCustomer.CurrentDebt;
                selectedCustomer.CurrentDebt = System.Math.Max(0, selectedCustomer.CurrentDebt - paymentAmount);

                // Record Debt Payment Entry
                var paymentEntry = new CustomerPayment
                {
                    CustomerId = selectedCustomer.Id,
                    CustomerName = selectedCustomer.Name,
                    PaymentDate = System.DateTime.Now,
                    AmountPaid = paymentAmount,
                    RemainingDebt = selectedCustomer.CurrentDebt,
                    PaymentMethod = method,
                    Notes = notes
                };

                _db.CustomerPayments.Add(paymentEntry);
                _db.Customers.Update(selectedCustomer);
                _db.SaveChanges();

                LoadCustomers();
                if (txtPaymentAmount != null) txtPaymentAmount.Clear();
                if (txtPaymentNotes != null) txtPaymentNotes.Clear();

                LoadPaymentHistory(selectedCustomer.Id, selectedCustomer.Name);

                MessageBox.Show($"========================================\n" +
                                $"      NIMAL HARDWARE STORE              \n" +
                                $"   CUSTOMER DEBT PAYMENT RECEIPT        \n" +
                                $"========================================\n" +
                                $"Date: {paymentEntry.PaymentDate:yyyy-MM-dd HH:mm}\n" +
                                $"Customer: {selectedCustomer.Name}\n" +
                                $"Payment Method: {method}\n" +
                                $"Payment Note: {notes}\n" +
                                $"----------------------------------------\n" +
                                $"Previous Debt: Rs. {previousDebt:F2}\n" +
                                $"AMOUNT PAID:   Rs. {paymentAmount:F2}\n" +
                                $"REMAINING DEBT: Rs. {selectedCustomer.CurrentDebt:F2}\n" +
                                $"========================================\n" +
                                $"      Thank you for your payment!       ",
                                "Payment Applied", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please select a customer account from the table first.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnPrintStatement_Click(object sender, RoutedEventArgs e)
        {
            if (dgCustomers.SelectedItem is Customer selectedCustomer)
            {
                var payments = _db.CustomerPayments
                                  .Where(p => p.CustomerId == selectedCustomer.Id)
                                  .OrderByDescending(p => p.PaymentDate)
                                  .Take(10)
                                  .ToList();

                string historySummary = "";
                if (payments.Count == 0)
                {
                    historySummary = "No debt payment transactions recorded yet.";
                }
                else
                {
                    foreach (var p in payments)
                    {
                        historySummary += $"{p.PaymentDate:yyyy-MM-dd HH:mm} | Paid: Rs. {p.AmountPaid:F2} | Rem: Rs. {p.RemainingDebt:F2} | Method: {p.PaymentMethod}\n";
                    }
                }

                MessageBox.Show($"========================================\n" +
                                $"       NIMAL HARDWARE STORE             \n" +
                                $"     CUSTOMER ACCOUNT STATEMENT         \n" +
                                $"========================================\n" +
                                $"Date: {System.DateTime.Now:yyyy-MM-dd HH:mm}\n" +
                                $"Account ID: #{selectedCustomer.Id}\n" +
                                $"Customer: {selectedCustomer.Name}\n" +
                                $"Phone: {selectedCustomer.Phone}\n" +
                                $"Loyalty Discount: {selectedCustomer.CustomerLoyaltyDiscount}%\n" +
                                $"----------------------------------------\n" +
                                $"Credit Limit:  Rs. {selectedCustomer.CreditLimit:F2}\n" +
                                $"Current Debt:  Rs. {selectedCustomer.CurrentDebt:F2}\n" +
                                $"Avail Credit:  Rs. {selectedCustomer.AvailableCredit:F2}\n" +
                                $"Debt Status:   {selectedCustomer.DebtStatus}\n" +
                                $"----------------------------------------\n" +
                                $"RECENT PAYMENT TRANSACTIONS:\n" +
                                $"{historySummary}" +
                                $"========================================\n",
                                "Customer Statement", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please select a customer to view their statement.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}