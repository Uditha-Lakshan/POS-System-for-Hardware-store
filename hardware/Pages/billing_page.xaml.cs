using HardwareShop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HardwareShop.Pages
{
    public partial class BillingPage : Page
    {
        private ObservableCollection<BillItem> _cart = new ObservableCollection<BillItem>();
        private ShopDbContext _db = new ShopDbContext();
        private Item? _selectedInventoryItem = null;

        public BillingPage()
        {
            InitializeComponent();
            dgClientBill.ItemsSource = _cart;
            LoadData();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _db = new ShopDbContext();
            LoadData();
        }

        private void LoadData()
        {
            _db = new ShopDbContext();
            FilterInventory();

            var categories = _db.Categories.ToList();
            categories.Insert(0, new Category { CatCode = 0, CatName = "-- All Categories --" });
            cmbFilterCategory.ItemsSource = categories;
            cmbFilterCategory.SelectedIndex = 0;

            PopulateSubCategoriesFilter(0);

            var customers = _db.Customers.ToList();
            customers.Insert(0, new Customer { Id = 0, Name = "-- Walk-in / Guest Customer --", CustomerLoyaltyDiscount = 0, CreditLimit = 0, CurrentDebt = 0 });
            cmbCustomers.ItemsSource = customers;
            cmbCustomers.SelectedIndex = 0;
        }

        private void PopulateSubCategoriesFilter(int mainCatId)
        {
            if (cmbFilterSubCategory == null) return;

            var subCats = mainCatId > 0
                ? _db.SubCategories.Where(s => s.CategoryId == mainCatId).ToList()
                : _db.SubCategories.ToList();

            subCats.Insert(0, new SubCategory { SubCatCode = 0, SubCatName = "-- All Sub-Categories --" });
            cmbFilterSubCategory.ItemsSource = subCats;
            cmbFilterSubCategory.SelectedIndex = 0;
        }

        private void FilterInventory()
        {
            var query = _db.Items.Include(i => i.Category)
                                 .Include(i => i.SubCategory)
                                 .AsQueryable();

            if (cmbFilterCategory?.SelectedValue is int catId && catId > 0)
            {
                query = query.Where(i => i.CategoryId == catId);
            }

            if (cmbFilterSubCategory?.SelectedValue is int subCatId && subCatId > 0)
            {
                query = query.Where(i => i.SubCategoryId == subCatId);
            }

            string searchText = txtSearchInventory?.Text?.Trim() ?? "";
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                if (int.TryParse(searchText, out int codeSearch))
                {
                    query = query.Where(i => i.ItName.ToLower().Contains(searchText.ToLower()) || i.ItCode == codeSearch);
                }
                else
                {
                    query = query.Where(i => i.ItName.ToLower().Contains(searchText.ToLower()) ||
                                             (i.SubCategory != null && i.SubCategory.SubCatName.ToLower().Contains(searchText.ToLower())));
                }
            }

            dgInventory.ItemsSource = query.ToList();
        }

        private void TxtSearchInventory_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterInventory();
        }

        private void CmbFilterCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int mainCatId = cmbFilterCategory?.SelectedValue is int id ? id : 0;
            PopulateSubCategoriesFilter(mainCatId);
            FilterInventory();
        }

        private void CmbFilterSubCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterInventory();
        }

        private void DgInventory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgInventory.SelectedItem is Item selectedItem)
            {
                _selectedInventoryItem = selectedItem;
                txtItemSearch.Text = selectedItem.ItName;
                txtPrice.Text = selectedItem.ItPrice.ToString("F2");
                txtQty.Text = "1";
                // Auto pre-fill item's allowed max discount
                txtDiscount.Text = selectedItem.MaxItemDiscount.ToString("F2");
            }
        }

        private void CmbCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCustomers?.SelectedItem is Customer selectedCustomer && selectedCustomer.Id > 0)
            {
                if (panelCustomerCreditInfo != null) panelCustomerCreditInfo.Visibility = Visibility.Visible;
                if (txtCustCurrentDebt != null) txtCustCurrentDebt.Text = $"Rs. {selectedCustomer.CurrentDebt:F2}";
                if (txtCustCreditLimit != null) txtCustCreditLimit.Text = $"Rs. {selectedCustomer.CreditLimit:F2}";
                if (txtCustAvailableCredit != null) txtCustAvailableCredit.Text = $"Rs. {selectedCustomer.AvailableCredit:F2}";
            }
            else
            {
                if (panelCustomerCreditInfo != null) panelCustomerCreditInfo.Visibility = Visibility.Collapsed;
            }

            UpdateTotal();
        }
        private void BtnAddToBill_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedInventoryItem == null)
            {
                MessageBox.Show("Please select an item from the inventory table first.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter a valid unit price.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtQty.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Please enter a valid quantity greater than 0.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtDiscount.Text, out decimal discount) || discount < 0)
            {
                MessageBox.Show("Please enter a valid discount amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Enforce max item discount limit
            if (_selectedInventoryItem.MaxItemDiscount > 0 && discount > _selectedInventoryItem.MaxItemDiscount)
            {
                discount = _selectedInventoryItem.MaxItemDiscount;
                txtDiscount.Text = discount.ToString("F2");
                MessageBox.Show($"Item discount of Rs. {discount:F2} exceeds maximum allowed discount of Rs. {_selectedInventoryItem.MaxItemDiscount:F2} for '{_selectedInventoryItem.ItName}'. It has been capped at Rs. {_selectedInventoryItem.MaxItemDiscount:F2}.", "Discount Capped", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Check inventory stock limit
            var dbItem = _db.Items.FirstOrDefault(i => i.ItCode == _selectedInventoryItem.ItCode);
            if (dbItem != null)
            {
                int existingQtyInCart = _cart.Where(i => i.ItemId == dbItem.ItCode).Sum(i => i.Qty);
                if (existingQtyInCart + qty > dbItem.ItStock)
                {
                    MessageBox.Show($"Cannot add {qty} units. Only {dbItem.ItStock - existingQtyInCart} remaining in stock!", "Insufficient Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var existingItem = _cart.FirstOrDefault(i => i.ItemId == _selectedInventoryItem.ItCode && i.Price == price);
            if (existingItem != null)
            {
                existingItem.Qty += qty;
                existingItem.AppliedDiscount += (discount * qty);
            }
            else
            {
                _cart.Add(new BillItem
                {
                    ItemId = _selectedInventoryItem.ItCode,
                    ItemName = _selectedInventoryItem.ItName,
                    Price = price,
                    Qty = qty,
                    AppliedDiscount = discount * qty
                });
            }

            UpdateTotal();
            ClearItemInputs();
        }

        private void BtnRemoveCartItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is BillItem item)
            {
                _cart.Remove(item);
                UpdateTotal();
            }
        }

        private void BtnClearCart_Click(object sender, RoutedEventArgs e)
        {
            _cart.Clear();
            UpdateTotal();
        }

        private void ClearItemInputs()
        {
            _selectedInventoryItem = null;
            txtItemSearch.Clear();
            txtPrice.Clear();
            txtQty.Text = "1";
            txtDiscount.Text = "0";
            dgInventory.SelectedItem = null;
        }

        private void UpdateTotal()
        {
            decimal subtotal = _cart.Sum(item => item.Price * item.Qty);
            decimal itemDiscountTotal = _cart.Sum(item => item.AppliedDiscount);
            decimal subtotalAfterItemDiscount = Math.Max(0, subtotal - itemDiscountTotal);

            decimal loyaltyDiscountPercent = 0;
            decimal loyaltyDiscountAmount = 0;

            if (cmbCustomers?.SelectedItem is Customer selectedCustomer && selectedCustomer.Id > 0)
            {
                loyaltyDiscountPercent = selectedCustomer.CustomerLoyaltyDiscount;
                if (loyaltyDiscountPercent > 0)
                {
                    loyaltyDiscountAmount = subtotalAfterItemDiscount * (loyaltyDiscountPercent / 100m);
                }
            }

            decimal grandTotal = Math.Max(0, subtotalAfterItemDiscount - loyaltyDiscountAmount);

            if (txtSubTotal != null) txtSubTotal.Text = $"Rs. {subtotal:F2}";
            if (txtItemDiscountTotal != null) txtItemDiscountTotal.Text = $"-Rs. {itemDiscountTotal:F2}";
            if (lblLoyaltyDiscountTitle != null) lblLoyaltyDiscountTitle.Text = $"Customer Loyalty ({loyaltyDiscountPercent}%):";
            if (txtLoyaltyDiscountTotal != null) txtLoyaltyDiscountTotal.Text = $"-Rs. {loyaltyDiscountAmount:F2}";
            if (txtTotal != null) txtTotal.Text = $"Rs. {grandTotal:F2}";

            CalculateChange();
        }

        private void PaymentMode_Changed(object sender, RoutedEventArgs e)
        {
            if (panelCash == null || txtDebtNote == null) return;

            if (rbCash?.IsChecked == true)
            {
                panelCash.Visibility = Visibility.Visible;
                txtDebtNote.Visibility = Visibility.Collapsed;
            }
            else if (rbCard?.IsChecked == true)
            {
                panelCash.Visibility = Visibility.Collapsed;
                txtDebtNote.Visibility = Visibility.Collapsed;
            }
            else if (rbDebt?.IsChecked == true)
            {
                panelCash.Visibility = Visibility.Collapsed;
                txtDebtNote.Visibility = Visibility.Visible;
            }
        }

        private void TxtCashReceived_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateChange();
        }

        private void CalculateChange()
        {
            if (txtChangeReturn == null) return;

            decimal grandTotal = GetGrandTotal();
            decimal cashReceived = decimal.TryParse(txtCashReceived?.Text, out decimal cash) ? Math.Max(0, cash) : 0;
            decimal diff = cashReceived - grandTotal;

            if (diff >= 0)
            {
                txtChangeReturn.Text = $"Rs. {diff:F2}";
                txtChangeReturn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0D9488"));
            }
            else
            {
                decimal unpaidDebt = Math.Abs(diff);
                if (cmbCustomers?.SelectedItem is Customer selectedCust && selectedCust.Id > 0)
                {
                    txtChangeReturn.Text = $"Debt: +Rs. {unpaidDebt:F2}";
                    txtChangeReturn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#D97706"));
                }
                else
                {
                    txtChangeReturn.Text = $"Short: -Rs. {unpaidDebt:F2}";
                    txtChangeReturn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DC2626"));
                }
            }
        }

        private decimal GetGrandTotal()
        {
            decimal subtotal = _cart.Sum(item => item.Price * item.Qty);
            decimal itemDiscountTotal = _cart.Sum(item => item.AppliedDiscount);
            decimal subtotalAfterItemDiscount = Math.Max(0, subtotal - itemDiscountTotal);

            decimal loyaltyDiscountPercent = 0;
            if (cmbCustomers?.SelectedItem is Customer selectedCustomer && selectedCustomer.Id > 0)
            {
                loyaltyDiscountPercent = selectedCustomer.CustomerLoyaltyDiscount;
            }
            decimal loyaltyDiscountAmount = subtotalAfterItemDiscount * (loyaltyDiscountPercent / 100m);
            return Math.Max(0, subtotalAfterItemDiscount - loyaltyDiscountAmount);
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("The cart is empty. Add items before checking out.", "Empty Cart", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            decimal subtotal = _cart.Sum(i => i.Price * i.Qty);
            decimal itemDiscountTotal = _cart.Sum(i => i.AppliedDiscount);
            decimal subtotalAfterItemDisc = Math.Max(0, subtotal - itemDiscountTotal);

            Customer? selectedCustomer = cmbCustomers.SelectedItem as Customer;
            int? customerId = (selectedCustomer != null && selectedCustomer.Id > 0) ? selectedCustomer.Id : null;
            string customerName = (selectedCustomer != null && selectedCustomer.Id > 0) ? selectedCustomer.Name : "Walk-in Customer";
            decimal loyaltyDiscountPercent = (selectedCustomer != null && selectedCustomer.Id > 0) ? selectedCustomer.CustomerLoyaltyDiscount : 0;
            decimal loyaltyDiscountAmount = subtotalAfterItemDisc * (loyaltyDiscountPercent / 100m);

            decimal grandTotal = Math.Max(0, subtotalAfterItemDisc - loyaltyDiscountAmount);
            decimal totalDiscountsAll = itemDiscountTotal + loyaltyDiscountAmount;

            string paymentMethod = "Cash";
            decimal cashPaid = 0;
            decimal debtAdded = 0;

            if (rbCard?.IsChecked == true)
            {
                paymentMethod = "Card";
            }
            else if (rbDebt?.IsChecked == true)
            {
                paymentMethod = "Customer Debt";

                if (selectedCustomer == null || selectedCustomer.Id == 0)
                {
                    MessageBox.Show("Please select a valid customer account from the dropdown for customer credit/debt billing.", "Customer Account Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dbCustomer = _db.Customers.FirstOrDefault(c => c.Id == selectedCustomer.Id);
                if (dbCustomer == null)
                {
                    MessageBox.Show("Selected customer account could not be found in database.", "Customer Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (dbCustomer.CurrentDebt + grandTotal > dbCustomer.CreditLimit)
                {
                    MessageBox.Show($"Transaction Failed!\n\nCharging Rs. {grandTotal:F2} exceeds {dbCustomer.Name}'s credit limit.\nCurrent Debt: Rs. {dbCustomer.CurrentDebt:F2}\nCredit Limit: Rs. {dbCustomer.CreditLimit:F2}", "Credit Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                debtAdded = grandTotal;
                dbCustomer.CurrentDebt += debtAdded;
                selectedCustomer.CurrentDebt = dbCustomer.CurrentDebt;
            }
            else // Cash payment option selected
            {
                cashPaid = decimal.TryParse(txtCashReceived?.Text, out decimal c) ? Math.Max(0, c) : 0;

                if (cashPaid < grandTotal)
                {
                    decimal unpaidBalance = grandTotal - cashPaid;

                    if (selectedCustomer == null || selectedCustomer.Id == 0)
                    {
                        MessageBox.Show($"Cash received (Rs. {cashPaid:F2}) is less than the Grand Total (Rs. {grandTotal:F2}).\n\nTo add the remaining unpaid balance of Rs. {unpaidBalance:F2} as customer debt, please select a registered customer account from the dropdown list.", "Customer Account Required for Debt", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var dbCustomer = _db.Customers.FirstOrDefault(cust => cust.Id == selectedCustomer.Id);
                    if (dbCustomer == null)
                    {
                        MessageBox.Show("Selected customer account could not be found in database.", "Customer Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Check customer credit limit
                    if (dbCustomer.CurrentDebt + unpaidBalance > dbCustomer.CreditLimit)
                    {
                        MessageBox.Show($"Transaction Failed!\n\nAdding unpaid balance of Rs. {unpaidBalance:F2} exceeds {dbCustomer.Name}'s credit limit.\nCurrent Debt: Rs. {dbCustomer.CurrentDebt:F2}\nCredit Limit: Rs. {dbCustomer.CreditLimit:F2}", "Credit Limit Exceeded", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    debtAdded = unpaidBalance;
                    dbCustomer.CurrentDebt += debtAdded;
                    selectedCustomer.CurrentDebt = dbCustomer.CurrentDebt;

                    paymentMethod = cashPaid > 0
                        ? $"Cash (Rs. {cashPaid:F2}) + Debt (Rs. {debtAdded:F2})"
                        : "Customer Debt (Unpaid Cash)";
                }
                else
                {
                    paymentMethod = "Cash";
                }
            }

            // Create Sale Record
            var sale = new Sale
            {
                SaleDate = DateTime.Now,
                PaymentMethod = paymentMethod,
                SubTotal = subtotal,
                Discount = totalDiscountsAll,
                TotalAmount = grandTotal,
                CustomerId = customerId,
                CustomerName = customerName
            };

            // Process Stock Deductions & Sale Details
            foreach (var cartItem in _cart)
            {
                sale.SaleDetails.Add(new SaleDetail
                {
                    ItemId = cartItem.ItemId,
                    ItemName = cartItem.ItemName,
                    UnitPrice = cartItem.Price,
                    Quantity = cartItem.Qty,
                    Discount = cartItem.AppliedDiscount,
                    TotalPrice = cartItem.Total
                });

                // Deduct stock in DB if item exists
                if (cartItem.ItemId > 0)
                {
                    var itemInDb = _db.Items.FirstOrDefault(i => i.ItCode == cartItem.ItemId);
                    if (itemInDb != null)
                    {
                        itemInDb.ItStock = Math.Max(0, itemInDb.ItStock - cartItem.Qty);
                    }
                }
            }

            _db.Sales.Add(sale);
            _db.SaveChanges(); // Save all changes atomically

            // Build Receipt / Confirmation Dialog
            string changeInfo = "";
            if (rbCash?.IsChecked == true && cashPaid >= grandTotal)
            {
                changeInfo = $"\nCash Paid: Rs. {cashPaid:F2}\nChange Returned: Rs. {Math.Max(0, cashPaid - grandTotal):F2}";
            }
            else if (debtAdded > 0)
            {
                changeInfo = $"\nCash Paid: Rs. {cashPaid:F2}\nAdded to Customer Debt: Rs. {debtAdded:F2}\nNew Total Debt: Rs. {selectedCustomer?.CurrentDebt:F2}";
            }

            string loyaltyText = loyaltyDiscountPercent > 0
                ? $"\nCustomer Loyalty Disc ({loyaltyDiscountPercent}%): -Rs. {loyaltyDiscountAmount:F2}"
                : "";

            MessageBox.Show($"========================================\n" +
                            $"        NIMAL HARDWARE STORE            \n" +
                            $"             SALES RECEIPT              \n" +
                            $"========================================\n" +
                            $"Invoice ID: #{sale.Id}\n" +
                            $"Date: {sale.SaleDate:yyyy-MM-dd HH:mm}\n" +
                            $"Customer: {customerName}\n" +
                            $"Payment Method: {paymentMethod}\n" +
                            $"----------------------------------------\n" +
                            $"Cart Items: {_cart.Count}\n" +
                            $"Cart Subtotal: Rs. {subtotal:F2}\n" +
                            $"Item Discounts: -Rs. {itemDiscountTotal:F2}" +
                            $"{loyaltyText}\n" +
                            $"----------------------------------------\n" +
                            $"GRAND TOTAL: Rs. {grandTotal:F2}" +
                            $"{changeInfo}\n" +
                            $"========================================\n" +
                            $"     Thank you for your visit!          ",
                            "Checkout Successful - Nimal Hardware Store", MessageBoxButton.OK, MessageBoxImage.Information);

            // Clear Cart & Refresh Inventory
            _cart.Clear();
            txtCashReceived?.Clear();
            txtChangeReturn?.Clear();
            cmbCustomers.SelectedIndex = 0;
            UpdateTotal();
            LoadData();
        }
    }
}