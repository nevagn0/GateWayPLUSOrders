using ControlSystemOrders.Domain.Model;
using ControlSystemOrders.Domain.Repository;
using ControlSystemOrders.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ControlSystemOrders.Infrastructure.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDb _ordersDb;

    public OrderRepository(OrdersDb ordersDb)
    {
        _ordersDb =  ordersDb;
    }

    public async Task<Order> CreateOrder(Order order)
    {
        await _ordersDb.Orders.AddAsync(order);
        await _ordersDb.SaveChangesAsync();
        return order;
    }

    public async Task UpdateOrder(Order order)
    {
        _ordersDb.Orders.Update(order);
        await _ordersDb.SaveChangesAsync();
    }
    public async Task CancelOrder(Order order)
    {
        _ordersDb.Orders.Remove(order);
        await _ordersDb.SaveChangesAsync();
    }

    public async Task<Order> GetOrderById(Guid id)
    {
        return await _ordersDb.Orders.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Order>> GetAllUserOrders(Order order)
    {
        return await _ordersDb.Orders.Where(e => e.UserId == order.UserId).ToListAsync();
    }
}