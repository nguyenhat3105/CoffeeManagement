using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using CoffeeManagement.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CoffeeManagement
{
    public partial class OrderHistory: UserControl
    {
        private readonly OrderService _orderService;
        private List<Order> _orders = new();

        public OrderHistory()
        {
            InitializeComponent();

            var ctx = new CoffeeManagementDbContext();
            var dao = new OrderDAO(ctx);
            var repo = new OrderRepository(dao);
            _orderService = new OrderService(repo);

            LoadOrderHistory();
        }


        private void LoadOrderHistory()
        {
            var customer = AppSession.CurrentUser;
            if (customer == null)
            {
                MessageBox.Show("Bạn cần đăng nhập để xem lịch sử mua hàng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _orders = _orderService
                .GetAllOrders()
                .Where(o => o.CustomerId == customer.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            OrdersList.ItemsSource = _orders;



            // ========== THỐNG KÊ ==========
            TxtTotalOrders.Text = _orders.Count.ToString();

            decimal totalSpent = _orders.Sum(o => o.TotalAmount);
            TxtTotalSpent.Text = $"{totalSpent:N0}đ";

            // Xác định món được mua nhiều nhất
            var favoriteItem = _orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(i => i.MenuItem.Name)
                .Select(g => new { Name = g.Key, Count = g.Sum(i => i.Quantity) })
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            TxtFavoriteItem.Text = favoriteItem?.Name ?? "Chưa có";
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
