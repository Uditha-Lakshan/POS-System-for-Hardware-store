using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HardwareShop.Models
{
    public class Category
    {
        [Key]
        public int CatCode { get; set; }
        public string CatName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CategoryDiscountPercentage { get; set; }

        public List<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
        public List<Item> Items { get; set; } = new List<Item>();

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int ItemCount { get; set; }
    }

    public class SubCategory
    {
        [Key]
        public int SubCatCode { get; set; }
        public string SubCatName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string CategoryName => Category?.CatName ?? "Unassigned";

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int ItemCount { get; set; }
    }

    public class Item
    {
        [Key]
        public int ItCode { get; set; }
        public string ItName { get; set; } = string.Empty;
        public decimal ItPrice { get; set; }
        public int ItStock { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
        public decimal MaxItemDiscount { get; set; }
        public string Unit { get; set; } = "Pcs";

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int? SubCategoryId { get; set; }
        public SubCategory? SubCategory { get; set; }
    }

    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public decimal CustomerLoyaltyDiscount { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentDebt { get; set; }

        public decimal AvailableCredit => Math.Max(0, CreditLimit - CurrentDebt);
        public string DebtStatus => CurrentDebt <= 0 ? "Clear" : (CurrentDebt >= CreditLimit ? "Limit Reached" : "Active Debt");
    }

    public class CustomerPayment
    {
        [Key]
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public decimal AmountPaid { get; set; }
        public decimal RemainingDebt { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string Notes { get; set; } = string.Empty;
    }

    public class BillItem
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Qty { get; set; }
        public decimal AppliedDiscount { get; set; }
        public decimal Total => (Price * Qty) - AppliedDiscount;
    }

    public class Sale
    {
        [Key]
        public int Id { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.Now;
        public string PaymentMethod { get; set; } = "Cash";
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerName { get; set; } = "Walk-in Customer";
        public List<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    }

    public class SaleDetail
    {
        [Key]
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}