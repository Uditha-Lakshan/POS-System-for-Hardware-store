using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using HardwareShop.Models;

namespace HardwareShop
{
    public partial class LoginWindow : Window
    {
        private ShopDbContext _db = new();

        public LoginWindow()
        {
            InitializeComponent();
        }

        // Encrypts the password into a secure scramble
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            string hashedPwd = HashPassword(txtPassword.Password);
            var user = _db.Users.FirstOrDefault(u => u.Username == txtUsername.Text && u.PasswordHash == hashedPwd);

            if (user != null)
            {
                // Login successful! Open the Main POS Window
                MainWindow main = new MainWindow();
                main.Show();

                // Close the login window
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Please enter a username and password to register.");
                return;
            }

            // Check if username already exists
            if (_db.Users.Any(u => u.Username == txtUsername.Text))
            {
                MessageBox.Show("That username is already taken!");
                return;
            }

            // Create and save new user
            var newUser = new User
            {
                Username = txtUsername.Text,
                PasswordHash = HashPassword(txtPassword.Password)
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();

            MessageBox.Show("Registration successful! You can now click Login.");
        }
    }
}