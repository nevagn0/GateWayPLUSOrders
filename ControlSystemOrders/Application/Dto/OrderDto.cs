using ControlSystemOrders.Domain.Model;

namespace ControlSystemOrders.Application.Dto
{
    public class CreateOrderDto
    {
        public decimal Price { get; set; }
        public CountAndProduct CountAndProduct { get; set; } = null!;
    }
    
    public class UpdateOrderDto
    {
        public string? Status { get; set; }
        public decimal? Price { get; set; }
        public CountAndProduct? CountAndProduct { get; set; }
    }
    
    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Status { get; set; } = null!;
        public decimal Price { get; set; }
        public CountAndProduct CountAndProduct { get; set; } = null!;
        public DateTime DateCreate { get; set; }
        public DateTime DateUpdate { get; set; }
    }
}