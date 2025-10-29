using CoffeeManagement.BLL.Services;
using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoffeeManagement
{
    /// <summary>
    /// Interaction logic for StaffOrders.xaml
    /// </summary>
    public partial class StaffOrders : UserControl
    {
        private readonly OrderService _orderService;
        private List<Order> _orders = new();

        public StaffOrders()
        {
            InitializeComponent();

            // tạo DAO/Repo/Service (không DI)
            var ctx = new CoffeeManagementDbContext();
            var dao = new OrderDAO(ctx);
            var repo = new OrderRepository(dao);
            _orderService = new OrderService(repo);

            LoadProcessingOrders();
        }

        private void LoadProcessingOrders()
        {
            _orders = _orderService.GetAllOrders().Where(o => o.Status == 0).ToList();
            OrdersItemsControl.ItemsSource = _orders;
        }

        private void BtnMarkComplete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Order order)
            {
                var res = MessageBox.Show($"Mark order #{order.Id} complete and paid?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    _orderService.UpdateOrderStatus(order.Id, 1); // 1 = completed
                    _orderService.UpdateOrderPaymentStatus(order.Id, true, DateTime.Now);
                    MessageBox.Show($"Order #{order.Id} updated.", "OK", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadProcessingOrders();
                }
            }
        }

        private void BtnReject_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Order order)
            {
                var res = MessageBox.Show($"Cancel order #{order.Id}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.Yes)
                {
                    _orderService.UpdateOrderStatus(order.Id, 4); // 4 = cancelled
                    LoadProcessingOrders();
                }
            }
        }
    }
}
