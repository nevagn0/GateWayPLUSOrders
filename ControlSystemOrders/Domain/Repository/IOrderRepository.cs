using ControlSystemOrders.Domain.Model;

namespace ControlSystemOrders.Domain.Repository;

public interface IOrderRepository
{
    Task<Order> CreateOrder(Order order);
    
    Task UpdateOrder(Order order);
    
    Task CancelOrder(Order order);
    
    Task<IEnumerable<Order>> GetAllUserOrders(Order order);
        
    Task<Order> GetOrderById(Guid id);
}
