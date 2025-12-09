using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ControlSystemOrders.Domain.Model;
using ControlSystemOrders.Domain.Repository;
using ControlSystemOrders.Application.Dto;
using Microsoft.EntityFrameworkCore;

namespace ControlSystemOrders.Application.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public OrderService(IOrderRepository orderRepository, IHttpContextAccessor httpContextAccessor)
        {
            _orderRepository = orderRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        
        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            var userId = GetUserIdFromToken();
            
            ValidateOrderDto(dto);
            
            var order = new Order
            {
                UserId = userId,
                Status = "Pending",
                Price = dto.Price,
                CountAndProduct = dto.CountAndProduct,
                DateCreate = DateTime.UtcNow,
                DateUpdate = DateTime.UtcNow
            };
            
            var createdOrder = await _orderRepository.CreateOrder(order);
            return MapToDto(createdOrder);
        }
        
        public async Task<OrderDto> UpdateOrderAsync(Guid id, UpdateOrderDto dto)
        {
            var order = await _orderRepository.GetOrderById(id);
            if (order == null)
                throw new KeyNotFoundException($"Order with id {id} not found");
            
            CheckUserPermission(order.UserId);
            
            if (!string.IsNullOrEmpty(dto.Status))
                order.Status = dto.Status;
            
            if (dto.Price.HasValue)
                order.Price = dto.Price.Value;
            
            if (dto.CountAndProduct != null)
                order.CountAndProduct = dto.CountAndProduct;
            
            order.DateUpdate = DateTime.UtcNow;
            
            await _orderRepository.UpdateOrder(order);
            return MapToDto(order);
        }
        
        public async Task<bool> CancelOrderAsync(Guid id)
        {
            var order = await _orderRepository.GetOrderById(id);
            if (order == null)
                return false;
            
            CheckUserPermission(order.UserId);
            
            if (order.Status == "Cancelled" || order.Status == "Completed")
                throw new InvalidOperationException($"Cannot cancel order with status {order.Status}");
            
            await _orderRepository.CancelOrder(order);
            return true;
        }
        
        public async Task<OrderDto> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetOrderById(id);
            if (order == null)
                throw new KeyNotFoundException($"Order with id {id} not found");
            
            CheckUserPermission(order.UserId);
            
            return MapToDto(order);
        }
        
        public async Task<IEnumerable<OrderDto>> GetAllUserOrdersAsync(Guid userId)
        {
            var currentUserId = GetUserIdFromToken();
            if (currentUserId != userId)
                throw new UnauthorizedAccessException("Cannot view other user's orders");
            
            var filterOrder = new Order { UserId = userId };
            var orders = await _orderRepository.GetAllUserOrders(filterOrder);
            
            return orders.Select(MapToDto);
        }
        
        
        private Guid GetUserIdFromToken()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                throw new UnauthorizedAccessException("HttpContext недоступен");
            
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
                throw new UnauthorizedAccessException("User not authenticated or invalid user id");
        
            return userId;
        }
        
        private void CheckUserPermission(Guid orderUserId)
        {
            var currentUserId = GetUserIdFromToken();
            if (currentUserId != orderUserId)
                throw new UnauthorizedAccessException("You don't have permission to access this order");
        }
        
        private void ValidateOrderDto(CreateOrderDto dto)
        {
            if (dto.Price <= 0)
                throw new ArgumentException("Price must be greater than 0");
            
            if (dto.CountAndProduct == null)
                throw new ArgumentException("Product information is required");
            
            if (dto.CountAndProduct.Count <= 0)
                throw new ArgumentException("Product count must be greater than 0");
            
            if (string.IsNullOrWhiteSpace(dto.CountAndProduct.Name))
                throw new ArgumentException("Product name is required");
        }
        
        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                Status = order.Status,
                Price = order.Price,
                CountAndProduct = order.CountAndProduct,
                DateCreate = order.DateCreate,
                DateUpdate = order.DateUpdate
            };
        }
    }
}