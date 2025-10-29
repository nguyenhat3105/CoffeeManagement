using CoffeeManagement.DAL.Models;
using System.Linq;
using System.Windows;

namespace CoffeeManagement
{
    public partial class OrderDetailDialog : Window
    {
        public OrderDetailDialog(Order order)
        {
            InitializeComponent();

            TxtOrderInfo.Text = $"Chi tiết đơn hàng #{order.Id} - {(order.IsPaid ? "Đã thanh toán" : "Chưa thanh toán")}";
            OrderItemsList.ItemsSource = order.OrderItems.Select(i => new
            {
                MenuItem = i.MenuItem,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.Quantity * i.UnitPrice
            }).ToList();

            TxtTotalAmount.Text = $"{order.TotalAmount:C0}";
        }
    }
}
