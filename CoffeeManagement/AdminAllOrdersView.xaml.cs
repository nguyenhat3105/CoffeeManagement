using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CoffeeManagement
{
    public partial class AdminAllOrdersView : UserControl
    {
        private readonly OrderService _orderService;
        private List<Order> _allOrders = new();
        private List<User> _allCustomers = new();

        public AdminAllOrdersView()
        {
            InitializeComponent();

            var ctx = new CoffeeManagementDbContext();
            var dao = new OrderDAO(ctx);
            var repo = new OrderRepository(dao);
            _orderService = new OrderService(repo);

            LoadCustomers();
            LoadOrders();
        }

        private void LoadCustomers()
        {
            try
            {
                using var ctx = new CoffeeManagementDbContext();
                var userDao = new UserDao(ctx);
                var userRepo = new UserRepository(userDao);
                var userService = new UserService(userRepo);
                _allCustomers = userService.GetByRole(3); // Giả sử RoleId 3 là khách hàng

                // Thêm lựa chọn "Tất cả"
                _allCustomers.Insert(0, new User { Id = 0, LastName = "Tất cả khách hàng" });
                CustomerFilter.ItemsSource = _allCustomers;
                CustomerFilter.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải khách hàng: {ex.Message}");
            }
        }

        private void LoadOrders()
        {
            try
            {
                _allOrders = _orderService.GetAllOrders();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải đơn hàng: {ex.Message}");
            }
        }

        private void ApplyFilters()
        {
            IEnumerable<Order> filtered = _allOrders;

            // Lọc theo khách hàng
            if (CustomerFilter.SelectedValue is int customerId && customerId != 0)
            {
                filtered = filtered.Where(o => o.CustomerId == customerId);
            }

            // Lọc theo ngày
            if (FromDatePicker.SelectedDate.HasValue)
                filtered = filtered.Where(o => o.CreatedAt >= FromDatePicker.SelectedDate.Value);

            if (ToDatePicker.SelectedDate.HasValue)
                filtered = filtered.Where(o => o.CreatedAt <= ToDatePicker.SelectedDate.Value.AddDays(1));

            // Cập nhật danh sách hiển thị
            OrdersList.ItemsSource = filtered.OrderByDescending(o => o.CreatedAt).ToList();
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            FromDatePicker.SelectedDate = null;
            ToDatePicker.SelectedDate = null;
            CustomerFilter.SelectedIndex = 0;
            LoadOrders();
        }

        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Order order)
            {
                var dlg = new OrderDetailDialog(order);
                dlg.ShowDialog();
            }
        }
    }
}
