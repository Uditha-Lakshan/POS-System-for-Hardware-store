using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HardwareShop.Models;

namespace HardwareShop.Pages
{
    public partial class CategoriesPage : Page
    {
        private ShopDbContext _db = new();

        public CategoriesPage()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories() => dgCategories.ItemsSource = _db.Categories.ToList();

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(txtCatDiscount.Text, out decimal discount) && !string.IsNullOrWhiteSpace(txtCatName.Text))
            {
                _db.Categories.Add(new Category { CatName = txtCatName.Text, CategoryDiscountPercentage = discount });
                _db.SaveChanges();
                LoadCategories();
                ClearInputs();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgCategories.SelectedItem is Category selectedCat && decimal.TryParse(txtCatDiscount.Text, out decimal discount))
            {
                selectedCat.CatName = txtCatName.Text;
                selectedCat.CategoryDiscountPercentage = discount;
                _db.Categories.Update(selectedCat);
                _db.SaveChanges();
                LoadCategories();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgCategories.SelectedItem is Category selectedCat)
            {
                _db.Categories.Remove(selectedCat);
                _db.SaveChanges();
                LoadCategories();
                ClearInputs();
            }
        }

        private void DgCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCategories.SelectedItem is Category selectedCat)
            {
                txtCatName.Text = selectedCat.CatName;
                txtCatDiscount.Text = selectedCat.CategoryDiscountPercentage.ToString();
            }
        }

        private void ClearInputs()
        {
            txtCatName.Clear();
            txtCatDiscount.Text = "0";
        }
    }
}