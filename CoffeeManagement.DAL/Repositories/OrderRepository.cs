using CoffeeManagement.DAL.DAO;
using CoffeeManagement.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeManagement.DAL.Repositories
{
    public class OrderRepository: IOrderRepository
    {
        private readonly IOrderDAO _orderDAO;
        public OrderRepository(IOrderDAO orderDAO)
        {
            _orderDAO = orderDAO;
        }

        public void CreateOrder(Order order, List<OrderItem> orderItems)
        {
            _orderDAO.CreateOrder(order, orderItems);
        }
        public bool DeleteOrder(int orderId)
        {
            return _orderDAO.DeleteOrder(orderId);
        }
        public List<Order> GetAllOrders()
        {
            return _orderDAO.GetAllOrders();
        }
        public Order? GetOrderByCustomerId(int customerId)
        {
            return _orderDAO.GetOrderByCustomerId(customerId);
        }
        public Order? GetOrderById(int orderId)
        {
            return _orderDAO.GetOrderById(orderId);
        }
        public Order? GetOrderByStaffId(int staffId)
        {
            return _orderDAO.GetOrderByStaffId(staffId);
        }
        public void UpdateOrderPaymentStatus(int orderId, bool isPaid, DateTime? paidAt)
        {
            _orderDAO.UpdateOrderPaymentStatus(orderId, isPaid, paidAt);
        }

        public void UpdateOrderStatus(int orderId, byte status)
        {
            _orderDAO.UpdateOrderStatus(orderId, status);
        }
        public void UpdateOrderStaff(int orderId, int staffId)
        {
            _orderDAO.UpdateOrderStaff(orderId, staffId);
        }
    }
}
