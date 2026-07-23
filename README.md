# 🛠️ Nimal Hardware Store - Point of Sale (POS) & Inventory System

An enterprise-grade, modern desktop **Point of Sale (POS) and Hardware Store Management System** built with **C# WPF (.NET 8)** and **Entity Framework Core (SQLite)**. Designed specifically for retail hardware stores, building materials suppliers, and equipment outlets to streamline sales checkout, customer debt tracking, inventory management, multi-tier categories, and executive analytics.

---

## ✨ Key Features

### ⚡ 1. Billing & POS Checkout
- **Fast POS Cart Operations**: Instant item search by name, product code, or sub-category.
- **Smart Item & Customer Discounts**: Automated max item discount capping and customer loyalty discount percentages (%).
- **Multi-Payment Options**: Cash, Card, and Customer Credit (Debt).
- **Auto Debt Conversion**: If cash received is $0 or less than the invoice total, the unpaid balance is automatically billed to the customer's credit account (subject to credit limit verification).
- **Instant Thermal / Dialog Receipts**: Formatted sales receipts with invoice ID, itemized breakdowns, total discounts, cash paid, change returned, or updated debt balances.

### 👥 2. Customer Credit & Debt Ledger System
- **Customer Profiles**: Full profile management with phone, loyalty discount %, credit limit (Rs.), and current debt balances.
- **Live Credit Badges**: Real-time credit limit, current debt, and available credit badges on the POS billing screen.
- **Partial & Full Debt Settlements**: Customer debt repayment with flexible payment methods (*Cash, Card, Bank Transfer, Cheque*) and payment reference notes.
- **Transaction Ledger & Statements**: Comprehensive payment history logs with instant statement generation.

### 📦 3. Hardware Inventory & Stock Control
- **Product Management**: Track item code, name, unit price (Rs.), stock quantity, manufacturer, unit of measure (*Pcs, Kg, Meters, Liters, Boxes, Packs*), and max allowed discount limits.
- **Real-Time Stock Deductions**: Automatic stock adjustments upon checkout with low stock warnings.

### 🏷️ 4. Two-Tier Category System (Main & Sub-Categories)
- **Main & Sub-Category Hierarchy**: Classify inventory into primary categories (*Plumbing, Electrical, Tools, Paints*) and linked sub-categories (*Pipes & Fittings, Taps, Power Tools, Hand Tools*).
- **Item Count Metrics & Notes**: Live counting of linked inventory items per category and sub-category with description fields.
- **Database Safeguards**: Blocked deletion of categories or sub-categories that currently contain linked inventory items.

### 📊 5. Executive Dashboard & Analytics
- **Stat Cards**: Real-time summary cards for Today's Revenue (Rs.), Total Stock Quantity, Registered Customers, and Total Outstanding Debt.
- **7-Day Revenue Bar Chart**: Dynamic visual bar chart tracking daily sales trends over the past week.
- **Low Stock Alerts**: Instant alert table highlighting items with stock **≤ 10 Pcs**.
- **Active Debtors Overview**: Table listing all active customer debtor accounts sorted by highest debt balance.

---

## 🛠️ Technology Stack

- **Framework**: WPF (Windows Presentation Foundation) on **.NET 8**
- **Language**: C# 12
- **Database & ORM**: SQLite with **Entity Framework Core 8**
- **UI Architecture**: XAML with Slate & Teal Modern Design Palette
- **Currency**: Sri Lankan Rupees (**Rs.**)

---

## 🚀 Getting Started

### Prerequisites
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (with *.NET Desktop Development* workload)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Installation & Run

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/Uditha-Lakshan/POS-System-for-Hardware-store.git
   cd POS-System-for-Hardware-store
   ```

2. **Restore Dependencies & Build**:
   ```bash
   dotnet restore
   dotnet build
   ```

3. **Run the Application**:
   ```bash
   dotnet run --project hardware/HardwareShop.csproj
   ```

> **Note**: The SQLite database file (`hardware.db`) will be automatically created and updated with auto-schema migrations on initial startup.

---

## 🗄️ Database Architecture

- **`Categories`**: Primary category hierarchy & default discounts.
- **`SubCategories`**: Sub-tier categories linked to parent categories.
- **`Items`**: Inventory items, pricing, stock levels, max discounts, and units.
- **`Customers`**: Customer accounts, credit limits, debt balances, and loyalty rates.
- **`CustomerPayments`**: Transaction ledger for debt repayments.
- **`Sales` & `SaleDetails`**: Transaction history, purchased items, discounts, and payment methods.
- **`Users`**: Authentication accounts for cashier/admin login.

---

## 📄 License
This project is open-source and available under the [MIT License](LICENSE).
