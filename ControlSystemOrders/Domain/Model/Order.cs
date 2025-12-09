namespace ControlSystemOrders.Domain.Model;
public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = null!;
    public decimal Price { get; set; }
    public CountAndProduct CountAndProduct { get; set; } = null!;
    
    public DateTime DateCreate { get; set; }
    public DateTime DateUpdate { get; set; }
}