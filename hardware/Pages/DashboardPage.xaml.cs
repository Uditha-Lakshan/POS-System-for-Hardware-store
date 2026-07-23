using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using HardwareShop.Models;

namespace HardwareShop.Pages
{
    public partial class DashboardPage : Page
    {
        private ShopDbContext _db = new();
        private List<DayRevenueData> _chartData = new();

        public DashboardPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _db = new ShopDbContext();
            LoadDashboardData();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _db = new ShopDbContext();
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                _db = new ShopDbContext();

                // Safely fetch sales
                var salesList = _db.Sales.ToList();
                DateTime today = DateTime.Today;
                decimal todaySales = salesList.Where(s => s.SaleDate.Date == today).Sum(s => s.TotalAmount);

                // Safely fetch items
                var itemsList = _db.Items.ToList();
                int totalItems = itemsList.Sum(i => i.ItStock);

                // Safely fetch customers
                var customersList = _db.Customers.ToList();
                int totalCustomers = customersList.Count;
                decimal totalDebt = customersList.Sum(c => c.CurrentDebt);

                // Update Stat Cards
                txtTodaySales.Text = $"Rs. {todaySales:F2}";
                txtTotalItems.Text = totalItems.ToString();
                txtTotalCustomers.Text = totalCustomers.ToString();
                txtTotalDebt.Text = $"Rs. {totalDebt:F2}";

                // 1. Build 7-Day Revenue Chart Data
                _chartData.Clear();
                decimal total7Days = 0;

                for (int i = 6; i >= 0; i--)
                {
                    DateTime date = today.AddDays(-i);
                    decimal dayRev = salesList.Where(s => s.SaleDate.Date == date).Sum(s => s.TotalAmount);
                    total7Days += dayRev;

                    _chartData.Add(new DayRevenueData
                    {
                        DayLabel = i == 0 ? "Today" : date.ToString("MMM dd"),
                        Revenue = dayRev
                    });
                }

                if (lbl7DayTotal != null) lbl7DayTotal.Text = $"Total 7-Day Revenue: Rs. {total7Days:F2}";
                RenderRevenueChart();

                // 2. Load Low Stock Items (threshold updated to stock <= 10 Pcs)
                var lowStockItems = itemsList.Where(i => i.ItStock <= 10).OrderBy(i => i.ItStock).ToList();
                dgLowStock.ItemsSource = lowStockItems;

                // 3. Load Active Debtors Accounts (CurrentDebt > 0)
                var activeDebtors = customersList.Where(c => c.CurrentDebt > 0).OrderByDescending(c => c.CurrentDebt).ToList();
                dgDebtors.ItemsSource = activeDebtors;
            }
            catch
            {
                txtTodaySales.Text = "Rs. 0.00";
                txtTotalItems.Text = "0";
                txtTotalCustomers.Text = "0";
                txtTotalDebt.Text = "Rs. 0.00";
            }
        }

        private void CanvasChart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderRevenueChart();
        }

        private void RenderRevenueChart()
        {
            if (canvasChart == null || gridChartLabels == null || _chartData.Count == 0) return;

            canvasChart.Children.Clear();
            gridChartLabels.Children.Clear();

            double canvasWidth = canvasChart.ActualWidth;
            double canvasHeight = canvasChart.ActualHeight;

            if (canvasWidth <= 0 || canvasHeight <= 0) return;

            decimal maxRevenue = _chartData.Max(d => d.Revenue);
            if (maxRevenue == 0) maxRevenue = 100m; // Default scale if zero sales

            double barWidth = Math.Max(25, (canvasWidth / 7.0) - 20);
            double stepX = canvasWidth / 7.0;

            for (int i = 0; i < _chartData.Count; i++)
            {
                var day = _chartData[i];

                // Calculate Bar Height
                double barHeight = (double)(day.Revenue / maxRevenue) * (canvasHeight - 35);
                if (barHeight < 4 && day.Revenue > 0) barHeight = 4;

                double left = (i * stepX) + (stepX - barWidth) / 2.0;
                double top = canvasHeight - barHeight;

                // Draw Bar Rectangle with Teal Gradient Fill
                Rectangle bar = new Rectangle
                {
                    Width = barWidth,
                    Height = barHeight > 0 ? barHeight : 2,
                    RadiusX = 4,
                    RadiusY = 4,
                    Fill = day.Revenue > 0
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0D9488"))
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CBD5E1")),
                    ToolTip = $"{day.DayLabel}: Rs. {day.Revenue:F2}"
                };

                Canvas.SetLeft(bar, left);
                Canvas.SetTop(bar, day.Revenue > 0 ? top : canvasHeight - 2);
                canvasChart.Children.Add(bar);

                // Add Amount Label above bar
                TextBlock txtAmount = new TextBlock
                {
                    Text = day.Revenue > 0 ? $"Rs.{day.Revenue:F0}" : "0",
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = day.Revenue > 0 ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0D9488")) : Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                txtAmount.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double textLeft = left + (barWidth - txtAmount.DesiredSize.Width) / 2.0;
                Canvas.SetLeft(txtAmount, Math.Max(0, textLeft));
                Canvas.SetTop(txtAmount, Math.Max(2, (day.Revenue > 0 ? top : canvasHeight - 2) - 18));
                canvasChart.Children.Add(txtAmount);

                // Add X-Axis Day Label below
                TextBlock txtDay = new TextBlock
                {
                    Text = day.DayLabel,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#475569")),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                };

                gridChartLabels.Children.Add(txtDay);
            }
        }

        private class DayRevenueData
        {
            public string DayLabel { get; set; } = string.Empty;
            public decimal Revenue { get; set; }
        }
    }
}