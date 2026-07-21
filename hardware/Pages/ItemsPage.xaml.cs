using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HardwareShop.Models;

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
            dgItems.ItemsSource = _db.Items.ToList();
            cmbCategory.ItemsSource = _db.Categories.ToList();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtPrice.Text, out decimal price) && int.TryParse(txtStock.Text, out int stock) &&
                decimal.TryParse(txtMaxDiscount.Text, out decimal discount) && cmbCategory.SelectedValue != null)
            {
                _db.Items.Add(new Item
                {
                    ItName = txtItemName.Text,
                    CategoryId = (int)cmbCategory.SelectedValue,
                    ItPrice = price,
                    ItStock = stock,
                    Manufacturer = txtManufacturer.Text,
                    MaxItemDiscount = discount
                });
                _db.SaveChanges();
                LoadData();
            }
            else { MessageBox.Show("Please check your inputs and ensure a category is selected."); }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgItems.SelectedItem is Item selectedItem && decimal.TryParse(txtPrice.Text, out decimal price) &&
                int.TryParse(txtStock.Text, out int stock) && decimal.TryParse(txtMaxDiscount.Text, out decimal discount))
            {
                selectedItem.ItName = txtItemName.Text;
                selectedItem.CategoryId = (int)cmbCategory.SelectedValue;
                selectedItem.ItPrice = price;
                selectedItem.ItStock = stock;
                selectedItem.Manufacturer = txtManufacturer.Text;
                selectedItem.MaxItemDiscount = discount;

                _db.Items.Update(selectedItem);
                _db.SaveChanges();
                LoadData();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgItems.SelectedItem is Item selectedItem)
            {
                _db.Items.Remove(selectedItem);
                _db.SaveChanges();
                LoadData();
            }
        }

        private void DgItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgItems.SelectedItem is Item selectedItem)
            {
                txtItemName.Text = selectedItem.ItName;
                txtPrice.Text = selectedItem.ItPrice.ToString();
                txtStock.Text = selectedItem.ItStock.ToString();
                txtManufacturer.Text = selectedItem.Manufacturer;
                txtMaxDiscount.Text = selectedItem.MaxItemDiscount.ToString();
                cmbCategory.SelectedValue = selectedItem.CategoryId;
            }
        }
    }
}