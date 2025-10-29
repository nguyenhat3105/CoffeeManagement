using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CoffeeManagement
{
    public partial class AdminDashboardView : UserControl
    {
        private readonly IOrderService orderService = new OrderService(new DAL.Repositories.OrderRepository(new DAL.DAO.OrderDAO(new CoffeeManagementDbContext())));

        public AdminDashboardView()
        {
            InitializeComponent();
            Loaded += AdminDashboardView_Loaded;
        }

        private void AdminDashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadDashboardData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải Dashboard: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDashboardData()
        {
            var orders = orderService.GetAllOrders()
                .Where(o => o.Status == 1) // chỉ tính đơn đã hoàn thành
                .ToList();

            if (!orders.Any()) return;

            TxtTotalOrders.Text = orders.Count.ToString();
            decimal totalRevenue = orders.Sum(o => o.TotalAmount);
            TxtTotalRevenue.Text = $"{totalRevenue:N0}₫";
            int totalItems = orders.Sum(o => o.OrderItems?.Sum(i => i.Quantity) ?? 0);
            TxtTotalItems.Text = totalItems.ToString();

            var peak = orders
                .GroupBy(o => o.CreatedAt.Hour)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();
            TxtPeakHour.Text = peak != null ? $"{peak.Key}:00" : "--:--";

            DrawRevenueChart(orders);
        }

        private void DrawRevenueChart(System.Collections.Generic.List<Order> orders)
        {
            RevenueCanvas.Children.Clear();

            var byDate = orders
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(o => o.TotalAmount) })
                .OrderBy(x => x.Date)
                .ToList();

            if (byDate.Count == 0) return;

            double canvasWidth = RevenueCanvas.ActualWidth > 0 ? RevenueCanvas.ActualWidth : 800;
            double canvasHeight = RevenueCanvas.ActualHeight > 0 ? RevenueCanvas.ActualHeight : 300;

            double maxRevenue = (double)byDate.Max(x => x.Revenue);
            double barWidth = canvasWidth / byDate.Count - 10;
            double x = 20;

            foreach (var item in byDate)
            {
                double barHeight = (double)item.Revenue / maxRevenue * (canvasHeight - 50);
                var rect = new Rectangle
                {
                    Width = barWidth,
                    Height = barHeight,
                    Fill = new SolidColorBrush(Color.FromRgb(255, 140, 0)),
                    RadiusX = 4,
                    RadiusY = 4
                };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, canvasHeight - barHeight - 30);
                RevenueCanvas.Children.Add(rect);

                var label = new TextBlock
                {
                    Text = item.Date.ToString("dd/MM"),
                    FontSize = 12,
                    Foreground = Brushes.Gray
                };
                Canvas.SetLeft(label, x);
                Canvas.SetTop(label, canvasHeight - 25);
                RevenueCanvas.Children.Add(label);

                x += barWidth + 10;
            }
        }
    }
}
