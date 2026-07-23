using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HardwareShop.Models;
using Microsoft.EntityFrameworkCore;

namespace HardwareShop.Pages
{
    public partial class ItemsPage : Page
    {
        private ShopDbContext _db = new();

        public ItemsPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            _db = new ShopDbContext();
            cmbCategory.ItemsSource = _db.Categories.ToList();
            FilterItems();
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCategory.SelectedValue is int catId)
            {
                cmbSubCategory.ItemsSource = _db.SubCategories.Where(s => s.CategoryId == catId).ToList();
            }
            else
            {
                cmbSubCategory.ItemsSource = null;
            }
        }

        private void FilterItems()
        {
            string filterText = txtSearch?.Text?.Trim()?.ToLower() ?? "";
            var query = _db.Items.Include(i => i.Category).Include(i => i.SubCategory).AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterText))
            {
                query = query.Where(i => i.ItName.ToLower().Contains(filterText) ||
                                         (i.Manufacturer != null && i.Manufacturer.ToLower().Contains(filterText)) ||
                                         (i.Category != null && i.Category.CatName.ToLower().Contains(filterText)) ||
                                         (i.SubCategory != null && i.SubCategory.SubCatName.ToLower().Contains(filterText)));
            }

            dgItems.ItemsSource = query.ToList();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterItems();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtItemName.Text))
            {
                MessageBox.Show("Please enter an item name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbCategory.SelectedValue == null)
            {
                MessageBox.Show("Please select a category.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (decimal.TryParse(txtPrice.Text, out decimal price) && price >= 0 &&
                int.TryParse(txtStock.Text, out int stock) && stock >= 0 &&
                decimal.TryParse(txtMaxDiscount.Text, out decimal discount) && discount >= 0)
            {
                string selectedUnit = (cmbUnit.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Pcs";
                int? subCatId = cmbSubCategory.SelectedValue as int?;

                _db.Items.Add(new Item
                {
                    ItName = txtItemName.Text,
                    CategoryId = (int)cmbCategory.SelectedValue,
                    SubCategoryId = subCatId,
                    ItPrice = price,
                    ItStock = stock,
                    Unit = selectedUnit,
                    Manufacturer = txtManufacturer.Text,
                    MaxItemDiscount = discount
                });
                _db.SaveChanges();
                LoadData();
                ClearForm();
                MessageBox.Show("Item added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please enter valid numeric values for Price, Stock, and Max Discount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgItems.SelectedItem is Item selectedItem)
            {
                if (cmbCategory.SelectedValue == null)
                {
                    MessageBox.Show("Please select a category.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (decimal.TryParse(txtPrice.Text, out decimal price) && price >= 0 &&
                    int.TryParse(txtStock.Text, out int stock) && stock >= 0 &&
                    decimal.TryParse(txtMaxDiscount.Text, out decimal discount) && discount >= 0)
                {
                    string selectedUnit = (cmbUnit.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Pcs";
                    int? subCatId = cmbSubCategory.SelectedValue as int?;

                    var dbItem = _db.Items.FirstOrDefault(i => i.ItCode == selectedItem.ItCode);
                    if (dbItem != null)
                    {
                        dbItem.ItName = txtItemName.Text;
                        dbItem.CategoryId = (int)cmbCategory.SelectedValue;
                        dbItem.SubCategoryId = subCatId;
                        dbItem.ItPrice = price;
                        dbItem.ItStock = stock;
                        dbItem.Unit = selectedUnit;
                        dbItem.Manufacturer = txtManufacturer.Text;
                        dbItem.MaxItemDiscount = discount;

                        _db.Items.Update(dbItem);
                        _db.SaveChanges();
                        LoadData();
                        ClearForm();
                        MessageBox.Show("Item updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select an item from the list to edit.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgItems.SelectedItem is Item selectedItem)
            {
                if (MessageBox.Show($"Are you sure you want to delete '{selectedItem.ItName}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var dbItem = _db.Items.FirstOrDefault(i => i.ItCode == selectedItem.ItCode);
                    if (dbItem != null)
                    {
                        _db.Items.Remove(dbItem);
                        _db.SaveChanges();
                        LoadData();
                        ClearForm();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select an item from the list to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtItemName.Clear();
            txtPrice.Clear();
            txtStock.Clear();
            txtManufacturer.Clear();
            txtMaxDiscount.Text = "0";
            cmbCategory.SelectedIndex = -1;
            cmbSubCategory.SelectedIndex = -1;
            cmbSubCategory.ItemsSource = null;
            cmbUnit.SelectedIndex = 0;
            dgItems.SelectedItem = null;
        }

        private void DgItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgItems.SelectedItem is Item selectedItem)
            {
                txtItemName.Text = selectedItem.ItName;
                txtPrice.Text = selectedItem.ItPrice.ToString("F2");
                txtStock.Text = selectedItem.ItStock.ToString();
                txtManufacturer.Text = selectedItem.Manufacturer;
                txtMaxDiscount.Text = selectedItem.MaxItemDiscount.ToString("F2");
                cmbCategory.SelectedValue = selectedItem.CategoryId;

                if (selectedItem.SubCategoryId != null)
                {
                    cmbSubCategory.ItemsSource = _db.SubCategories.Where(s => s.CategoryId == selectedItem.CategoryId).ToList();
                    cmbSubCategory.SelectedValue = selectedItem.SubCategoryId;
                }

                // Select matching unit
                foreach (ComboBoxItem item in cmbUnit.Items)
                {
                    if (item.Content.ToString() == selectedItem.Unit)
                    {
                        cmbUnit.SelectedItem = item;
                        break;
                    }
                }
            }
        }
    }
}