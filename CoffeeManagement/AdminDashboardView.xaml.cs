using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using CoffeeManagement.DAL.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
//using System.Windows.Shapes; // <-- ĐÃ XÓA: Không cần vẽ tay nữa

// *** THÊM MỚI: Using cho LiveCharts ***
using LiveCharts;
using LiveCharts.Wpf;

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
                MessageBox.Show($"Lỗi khi tải Dashboard: {ex.Message}\n\n(Bạn có chắc đã dùng .Include() để tải OrderItems và MenuItems chưa?)", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDashboardData()
        {
            // *** SỬA LỖI QUAN TRỌNG: Phải dùng hàm có .Include() ***
            // (Hãy đảm bảo bạn đã tạo hàm này trong Service/Repository)
            var allOrders = orderService.GetAllOrders();

            var orders = allOrders
                .Where(o => o.Status == 1) // chỉ tính đơn đã hoàn thành
                .ToList();

            // Nếu không có đơn nào, reset UI
            if (!orders.Any())
            {
                TxtTotalOrders.Text = "0";
                TxtTotalRevenue.Text = "0đ";
                TxtTotalItems.Text = "0";
                RevenueChart.Series.Clear();
                RevenueChart.AxisX[0].Labels = null;
                TopItemsChart.Series.Clear();
                return;
            }

            // 1. Nạp các thẻ thống kê
            TxtTotalOrders.Text = orders.Count.ToString();
            decimal totalRevenue = orders.Sum(o => o.TotalAmount);
            TxtTotalRevenue.Text = $"{totalRevenue:N0}₫";

            // (Logic này giờ sẽ hoạt động vì OrderItems đã được Include)
            int totalItems = orders.Sum(o => o.OrderItems?.Sum(i => i.Quantity) ?? 0);
            TxtTotalItems.Text = totalItems.ToString();

            // (Xóa TxtPeakHour vì nó không còn trong XAML mới)

            // 2. Nạp dữ liệu cho 2 biểu đồ
            LoadRevenueChart(orders);
            LoadTopItemsChart(orders);
        }

        // =============================================
        // HÀM MỚI: Nạp biểu đồ doanh thu
        // =============================================
        private void LoadRevenueChart(List<Order> orders)
        {
            // Lấy 7 ngày gần nhất
            var byDate = orders
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(o => o.TotalAmount) })
                .OrderByDescending(x => x.Date)
                .Take(7)
                .OrderBy(x => x.Date) // Sắp xếp lại
                .ToList();

            var revenueValues = new ChartValues<decimal>(byDate.Select(x => x.Revenue));
            var dateLabels = byDate.Select(x => x.Date.ToString("dd/MM")).ToList();

            RevenueChart.Series.Clear(); // Xóa dữ liệu cũ
            RevenueChart.Series.Add(new LineSeries
            {
                Title = "Doanh thu",
                Values = revenueValues,
                Stroke = (Brush)FindResource("RevenueBrush"), // Lấy màu từ Resources
                Fill = Brushes.Transparent,
                DataLabels = true,
                LabelPoint = chartPoint => $"{chartPoint.Y:N0}đ" // Hiển thị giá trị trên điểm
            });

            RevenueChart.AxisX[0].Labels = dateLabels;
            RevenueChart.AxisY[0].LabelFormatter = value => $"{value:N0}đ";
        }

        // =============================================
        // HÀM MỚI: Nạp biểu đồ tròn Top 5
        // =============================================
        private void LoadTopItemsChart(List<Order> orders)
        {
            // (Hàm này yêu cầu .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem))
            var topItems = orders
                .SelectMany(o => o.OrderItems) // Lấy TẤT CẢ OrderItems
                .GroupBy(oi => oi.MenuItem.Name) // Nhóm theo tên món
                .Select(g => new
                {
                    Name = g.Key,
                    Total = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.Total)
                .Take(5) // Lấy 5 món top
                .ToList();

            var seriesCollection = new SeriesCollection();

            foreach (var item in topItems)
            {
                seriesCollection.Add(new PieSeries
                {
                    Title = item.Name,
                    Values = new ChartValues<int> { item.Total },
                    DataLabels = true,
                    LabelPoint = chartPoint => $"{chartPoint.Participation:P0}" // Hiển thị %
                });
            }

            TopItemsChart.Series = seriesCollection;
        }


        // *** ĐÃ XÓA: Hàm DrawRevenueChart(List<Order> orders) ***
        // (Không cần thiết nữa vì LiveCharts tự vẽ)
    }
}