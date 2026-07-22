using System.ComponentModel.DataAnnotations;

namespace HardwareShop.Models
{
    public class Category
    {
        [Key]
        public int CatCode { get; set; }
        public string CatName { get; set; }
        public decimal CategoryDiscountPercentage { get; set; }
    }

    public class Item
    {
        [Key]
        public int ItCode { get; set; }
        public string ItName { get; set; }
        public decimal ItPrice { get; set; }
        public int ItStock { get; set; }
        public string Manufacturer { get; set; }
        public decimal MaxItemDiscount { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }

    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public decimal CustomerLoyaltyDiscount { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentDebt { get; set; }
    }

    public class BillItem
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public int Qty { get; set; }
        public decimal AppliedDiscount { get; set; }
        public decimal Total => (Price * Qty) - AppliedDiscount;
    }

    // --- ADD THE NEW USER CLASS RIGHT HERE BELOW BILLITEM ---
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }
}