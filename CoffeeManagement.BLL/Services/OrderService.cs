using CoffeeManagement.DAL.Models;
using CoffeeManagement.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeManagement.BLL.Services
{
    public class OrderService: IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        public void CreateOrder(Order order, List<OrderItem> orderItems)
        {
            _orderRepository.CreateOrder(order, orderItems);
        }
        public bool DeleteOrder(int orderId)
        {
            return _orderRepository.DeleteOrder(orderId);
        }
        public List<Order> GetAllOrders()
        {
            return _orderRepository.GetAllOrders();
        }
        public Order? GetOrderByCustomerId(int customerId)
        {
            return _orderRepository.GetOrderByCustomerId(customerId);
        }
        public Order? GetOrderById(int orderId)
        {
            return _orderRepository.GetOrderById(orderId);
        }
        public Order? GetOrderByStaffId(int staffId)
        {
            return _orderRepository.GetOrderByStaffId(staffId);
        }
        public void UpdateOrderPaymentStatus(int orderId, bool isPaid, DateTime? paidAt)
        {
            _orderRepository.UpdateOrderPaymentStatus(orderId, isPaid, paidAt);
        }
        public void UpdateOrderStatus(int orderId, byte status)
        {
            _orderRepository.UpdateOrderStatus(orderId, status);
        }
        public void UpdateOrderStaff(int orderId, int staffId)
        {
            _orderRepository.UpdateOrderStaff(orderId, staffId);
        }
    }
}
