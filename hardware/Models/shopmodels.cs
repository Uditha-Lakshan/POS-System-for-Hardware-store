using System.ComponentModel.DataAnnotations;

namespace HardwareShop.Models
{
    public class Category
    {
        [Key]
        public int CatCode { get; set; }
        public string CatName { get; set; } = string.Empty;
        public decimal CategoryDiscountPercentage { get; set; } // Discount on Category Page
    }

    public class Item
    {
        [Key]
        public int ItCode { get; set; }
        public string ItName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal ItPrice { get; set; }
        public int ItStock { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
        public decimal MaxItemDiscount { get; set; } // Discount on Items Page
    }

    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal CustomerLoyaltyDiscount { get; set; } // Discount on Customer Page

        // DEBT SYSTEM REQUIREMENTS
        public decimal CreditLimit { get; set; }
        public decimal CurrentDebt { get; set; }
    }

    public class BillItem
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Qty { get; set; }
        public decimal AppliedDiscount { get; set; } // Discount on Billing Page

        public decimal Total => (Price * Qty) - AppliedDiscount;
    }
}