using CoffeeManagement.DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeManagement.DAL.DAO
{
    public class OrderDAO : IOrderDAO
    {
        private readonly CoffeeManagementDbContext _context;

        public OrderDAO(CoffeeManagementDbContext context)
        {
            _context = context;
        }
        public Order CreateOrder(Order order, List<OrderItem> orderItems)
        {
            try
            {
                _context.Orders.Add(order);
                _context.SaveChanges();
                foreach (var item in orderItems)
                {
                    item.OrderId = order.Id;
                    _context.OrderItems.Add(item);
                }
                _context.SaveChanges();
                return order;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error when creating order: {ex.Message}");
            }
        }

        public bool DeleteOrder(int orderId)
        {
            var order = _context.Orders
                        .Include(o => o.OrderItems)
                        .FirstOrDefault(o => o.Id == orderId);

            if (order != null)
            {
                _context.OrderItems.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);
                return _context.SaveChanges() > 0;
            }

            return false;
        }

        public List<Order> GetAllOrders()
        {
            return _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();
        }

        public Order? GetOrderByCustomerId(int customerId)
        {
            return _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefault(o => o.CustomerId == customerId);
        }

        public Order? GetOrderById(int orderId)
        {
            return _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .FirstOrDefault(o => o.Id == orderId);
        }

        public Order? GetOrderByStaffId(int staffId)
        {
            return _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefault(o => o.StaffId == staffId);
        }

        public void UpdateOrderPaymentStatus(int orderId, bool isPaid, DateTime? paidAt)
        {
            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.IsPaid = isPaid;
                order.PaidAt = paidAt;
                order.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public void UpdateOrderStatus(int orderId, byte status)
        {
            var order = _context.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = status;
                order.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }
        }
    }
}
