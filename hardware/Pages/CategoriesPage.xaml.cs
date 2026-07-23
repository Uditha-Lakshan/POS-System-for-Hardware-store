using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using HardwareShop.Models;

namespace HardwareShop.Pages
{
    public partial class CategoriesPage : Page
    {
        private ShopDbContext _db = new();

        public CategoriesPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            FilterCategories();
            FilterSubCategories();
        }

        #region MAIN CATEGORIES LOGIC

        private void FilterCategories()
        {
            _db = new ShopDbContext();
            string filter = txtSearchCategory?.Text?.Trim().ToLower() ?? "";

            var categories = _db.Categories.ToList();
            var itemCounts = _db.Items.GroupBy(i => i.CategoryId).ToDictionary(g => g.Key, g => g.Count());

            foreach (var cat in categories)
            {
                cat.ItemCount = itemCounts.ContainsKey(cat.CatCode) ? itemCounts[cat.CatCode] : 0;
            }

            if (!string.IsNullOrWhiteSpace(filter))
            {
                categories = categories.Where(c => c.CatName.ToLower().Contains(filter) ||
                                                   (c.Description != null && c.Description.ToLower().Contains(filter)))
                                       .ToList();
            }

            dgCategories.ItemsSource = categories;
            cmbParentCategory.ItemsSource = _db.Categories.ToList();
        }

        private void TxtSearchCategory_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterCategories();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCatName.Text))
            {
                MessageBox.Show("Please enter a category name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (decimal.TryParse(txtCatDiscount.Text, out decimal discount) && discount >= 0)
            {
                var newCat = new Category
                {
                    CatName = txtCatName.Text.Trim(),
                    Description = txtDescription.Text?.Trim() ?? "",
                    CategoryDiscountPercentage = discount
                };

                _db.Categories.Add(newCat);
                _db.SaveChanges();
                LoadData();
                ClearInputs();
                MessageBox.Show($"Category '{newCat.CatName}' added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please enter a valid positive discount percentage.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgCategories.SelectedItem is Category selectedCat)
            {
                if (string.IsNullOrWhiteSpace(txtCatName.Text))
                {
                    MessageBox.Show("Please enter a category name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (decimal.TryParse(txtCatDiscount.Text, out decimal discount) && discount >= 0)
                {
                    var dbCat = _db.Categories.FirstOrDefault(c => c.CatCode == selectedCat.CatCode);
                    if (dbCat != null)
                    {
                        dbCat.CatName = txtCatName.Text.Trim();
                        dbCat.Description = txtDescription.Text?.Trim() ?? "";
                        dbCat.CategoryDiscountPercentage = discount;

                        _db.Categories.Update(dbCat);
                        _db.SaveChanges();
                        LoadData();
                        ClearInputs();
                        MessageBox.Show($"Category '{dbCat.CatName}' updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid positive discount percentage.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please select a category from the list to edit.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgCategories.SelectedItem is Category selectedCat)
            {
                int associatedItemsCount = _db.Items.Count(i => i.CategoryId == selectedCat.CatCode);
                if (associatedItemsCount > 0)
                {
                    MessageBox.Show($"Cannot delete category '{selectedCat.CatName}' because it currently has {associatedItemsCount} linked inventory item(s).\n\nPlease reassign or delete the items in the Items Inventory page first.", "Deletion Blocked", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show($"Are you sure you want to delete category '{selectedCat.CatName}'?", "Confirm Delete Category", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var dbCat = _db.Categories.FirstOrDefault(c => c.CatCode == selectedCat.CatCode);
                    if (dbCat != null)
                    {
                        _db.Categories.Remove(dbCat);
                        _db.SaveChanges();
                        LoadData();
                        ClearInputs();
                        MessageBox.Show("Category deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a category from the list to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearInputs();
        }

        private void DgCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCategories.SelectedItem is Category selectedCat)
            {
                txtCatName.Text = selectedCat.CatName;
                txtDescription.Text = selectedCat.Description;
                txtCatDiscount.Text = selectedCat.CategoryDiscountPercentage.ToString("F2");
            }
        }

        private void ClearInputs()
        {
            txtCatName.Clear();
            txtDescription.Clear();
            txtCatDiscount.Text = "0";
            if (txtSearchCategory != null) txtSearchCategory.Clear();
            dgCategories.SelectedItem = null;
        }

        #endregion

        #region SUB-CATEGORIES LOGIC

        private void FilterSubCategories()
        {
            _db = new ShopDbContext();
            string filter = txtSearchSubCategory?.Text?.Trim().ToLower() ?? "";

            var subCats = _db.SubCategories.Include(s => s.Category).ToList();
            var itemCounts = _db.Items.Where(i => i.SubCategoryId != null).GroupBy(i => i.SubCategoryId!.Value).ToDictionary(g => g.Key, g => g.Count());

            foreach (var sub in subCats)
            {
                sub.ItemCount = itemCounts.ContainsKey(sub.SubCatCode) ? itemCounts[sub.SubCatCode] : 0;
            }

            if (!string.IsNullOrWhiteSpace(filter))
            {
                subCats = subCats.Where(s => s.SubCatName.ToLower().Contains(filter) ||
                                             (s.Category != null && s.Category.CatName.ToLower().Contains(filter)) ||
                                             (s.Description != null && s.Description.ToLower().Contains(filter)))
                                 .ToList();
            }

            dgSubCategories.ItemsSource = subCats;
        }

        private void TxtSearchSubCategory_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterSubCategories();
        }

        private void BtnAddSubCat_Click(object sender, RoutedEventArgs e)
        {
            if (cmbParentCategory.SelectedItem is not Category parentCat)
            {
                MessageBox.Show("Please select a parent main category.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSubCatName.Text))
            {
                MessageBox.Show("Please enter a sub-category name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var subCat = new SubCategory
            {
                SubCatName = txtSubCatName.Text.Trim(),
                Description = txtSubCatDescription.Text?.Trim() ?? "",
                CategoryId = parentCat.CatCode
            };

            _db.SubCategories.Add(subCat);
            _db.SaveChanges();
            FilterSubCategories();
            ClearSubCatInputs();
            MessageBox.Show($"Sub-category '{subCat.SubCatName}' added successfully under '{parentCat.CatName}'!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnEditSubCat_Click(object sender, RoutedEventArgs e)
        {
            if (dgSubCategories.SelectedItem is SubCategory selectedSubCat)
            {
                if (cmbParentCategory.SelectedItem is not Category parentCat)
                {
                    MessageBox.Show("Please select a parent main category.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtSubCatName.Text))
                {
                    MessageBox.Show("Please enter a sub-category name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dbSub = _db.SubCategories.FirstOrDefault(s => s.SubCatCode == selectedSubCat.SubCatCode);
                if (dbSub != null)
                {
                    dbSub.SubCatName = txtSubCatName.Text.Trim();
                    dbSub.Description = txtSubCatDescription.Text?.Trim() ?? "";
                    dbSub.CategoryId = parentCat.CatCode;

                    _db.SubCategories.Update(dbSub);
                    _db.SaveChanges();
                    FilterSubCategories();
                    ClearSubCatInputs();
                    MessageBox.Show($"Sub-category '{dbSub.SubCatName}' updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a sub-category from the list to edit.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnDeleteSubCat_Click(object sender, RoutedEventArgs e)
        {
            if (dgSubCategories.SelectedItem is SubCategory selectedSubCat)
            {
                int associatedItems = _db.Items.Count(i => i.SubCategoryId == selectedSubCat.SubCatCode);
                if (associatedItems > 0)
                {
                    MessageBox.Show($"Cannot delete sub-category '{selectedSubCat.SubCatName}' because it currently has {associatedItems} linked item(s).\n\nPlease reassign or delete the items in the Inventory page first.", "Deletion Blocked", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show($"Are you sure you want to delete sub-category '{selectedSubCat.SubCatName}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var dbSub = _db.SubCategories.FirstOrDefault(s => s.SubCatCode == selectedSubCat.SubCatCode);
                    if (dbSub != null)
                    {
                        _db.SubCategories.Remove(dbSub);
                        _db.SaveChanges();
                        FilterSubCategories();
                        ClearSubCatInputs();
                        MessageBox.Show("Sub-category deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a sub-category from the list to delete.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnClearSubCat_Click(object sender, RoutedEventArgs e)
        {
            ClearSubCatInputs();
        }

        private void DgSubCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgSubCategories.SelectedItem is SubCategory selectedSubCat)
            {
                cmbParentCategory.SelectedValue = selectedSubCat.CategoryId;
                txtSubCatName.Text = selectedSubCat.SubCatName;
                txtSubCatDescription.Text = selectedSubCat.Description;
            }
        }

        private void ClearSubCatInputs()
        {
            cmbParentCategory.SelectedItem = null;
            txtSubCatName.Clear();
            txtSubCatDescription.Clear();
            if (txtSearchSubCategory != null) txtSearchSubCategory.Clear();
            dgSubCategories.SelectedItem = null;
        }

        #endregion
    }
}